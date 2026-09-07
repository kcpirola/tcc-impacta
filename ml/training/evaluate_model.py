"""
Script de avaliação dos modelos treinados.
Calcula acurácia, precisão, recall e F1-score, comparando com as metas do projeto
(acurácia > 95%, precisão > 94%, recall > 96%, F1 > 95%).

Além do relatório no console, grava:
    - models/metrics.json  : métricas de NB, RF e ensemble em formato legível por máquina
    - models/METRICS.md    : tabela pronta para colar no TCC

Uso:
    python -m training.evaluate_model --dataset data/dataset.csv
"""
import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

import joblib
import numpy as np
import sklearn
from scipy.sparse import hstack, csr_matrix
from sklearn.metrics import classification_report, confusion_matrix, precision_recall_fscore_support
from sklearn.model_selection import train_test_split

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config
from training.data_preprocessing import EmailPreprocessor
from training.feature_extraction import NUMERIC_FEATURE_NAMES, PhishingFeatureExtractor
from training.train_model import load_dataset, build_features, LABEL_ORDER

TARGET_METRICS = {
    "accuracy": 0.95,
    "precision": 0.94,
    "recall": 0.96,
    "f1": 0.95,
}


def evaluate(dataset_path: Path, report_md: Path | None = None) -> dict:
    """
    Avalia os modelos treinados no conjunto de teste (mesmo split 80/20 do treino) e
    retorna/persiste as métricas de Naive Bayes, Random Forest e do ensemble.
    """
    print("Carregando modelos treinados...")
    nb_model = joblib.load(Config.NAIVE_BAYES_MODEL_PATH)
    rf_model = joblib.load(Config.RANDOM_FOREST_MODEL_PATH)
    vectorizer = joblib.load(Config.TFIDF_VECTORIZER_PATH)
    scaler = joblib.load(Config.FEATURE_SCALER_PATH)
    label_encoder = joblib.load(Config.MODELS_DIR / "label_encoder.pkl")

    print(f"Carregando dataset de avaliação de {dataset_path}...")
    df = load_dataset(dataset_path)

    preprocessor = EmailPreprocessor()
    extractor = PhishingFeatureExtractor()
    cleaned_texts, numeric_matrix = build_features(df, preprocessor, extractor)

    y_true = label_encoder.transform(df["label"])
    tfidf_matrix = vectorizer.transform(cleaned_texts)
    scaled_numeric = scaler.transform(numeric_matrix)
    combined_features = hstack([tfidf_matrix, csr_matrix(scaled_numeric)]).tocsr()

    _, X_test, _, y_test = train_test_split(
        combined_features, y_true,
        test_size=Config.TEST_SIZE,
        random_state=Config.RANDOM_STATE,
        stratify=y_true,
    )

    # Índices de rótulo fixos (0..N-1). Passar `labels` explicitamente garante que as
    # métricas e o classification_report não quebrem quando o split de teste não contém
    # todas as classes.
    labels = list(range(len(label_encoder.classes_)))
    class_names = list(label_encoder.classes_)

    results: dict[str, dict] = {}

    print("\n" + "=" * 60)
    results["naive_bayes"] = _evaluate_model(
        nb_model, X_test, y_test, labels, class_names, "Naive Bayes"
    )
    print("\n" + "=" * 60)
    results["random_forest"] = _evaluate_model(
        rf_model, X_test, y_test, labels, class_names, "Random Forest"
    )
    print("\n" + "=" * 60)
    results["ensemble"] = _evaluate_ensemble(
        nb_model, rf_model, X_test, y_test, labels, class_names
    )

    report = _build_report(df, dataset_path, class_names, results)
    _write_metrics_json(report)
    if report_md is not None:
        _write_metrics_md(Path(report_md), report)

    return report


def _compute_metrics(y_test, y_pred, labels: list[int], class_names: list[str]) -> dict:
    """Métricas ponderadas + por classe + matriz de confusão, robustas a classe ausente."""
    precision, recall, f1, _ = precision_recall_fscore_support(
        y_test, y_pred, average="weighted", labels=labels, zero_division=0
    )
    p_c, r_c, f_c, s_c = precision_recall_fscore_support(
        y_test, y_pred, average=None, labels=labels, zero_division=0
    )
    accuracy = float(np.mean(np.asarray(y_pred) == np.asarray(y_test)))
    cm = confusion_matrix(y_test, y_pred, labels=labels)

    return {
        "accuracy": accuracy,
        "precision": float(precision),
        "recall": float(recall),
        "f1": float(f1),
        "per_class": {
            class_names[i]: {
                "precision": float(p_c[i]),
                "recall": float(r_c[i]),
                "f1": float(f_c[i]),
                "support": int(s_c[i]),
            }
            for i in range(len(class_names))
        },
        "confusion_matrix": cm.tolist(),
        "meets_targets": {
            "accuracy": bool(accuracy >= TARGET_METRICS["accuracy"]),
            "precision": bool(precision >= TARGET_METRICS["precision"]),
            "recall": bool(recall >= TARGET_METRICS["recall"]),
            "f1": bool(f1 >= TARGET_METRICS["f1"]),
        },
    }


def _evaluate_model(model, X_test, y_test, labels, class_names, model_name: str) -> dict:
    y_pred = model.predict(X_test)
    metrics = _compute_metrics(y_test, y_pred, labels, class_names)

    print(f"\n### {model_name} ###")
    _print_metrics(metrics)
    print("\nRelatório detalhado por classe:")
    print(classification_report(
        y_test, y_pred, labels=labels, target_names=class_names, zero_division=0
    ))
    print("Matriz de confusão:")
    print(np.array(metrics["confusion_matrix"]))

    _print_pass_fail(metrics)
    return metrics


def _evaluate_ensemble(nb_model, rf_model, X_test, y_test, labels, class_names) -> dict:
    """Avalia o ensemble ponderado por argmax (mesma lógica usada em produção)."""
    nb_proba = nb_model.predict_proba(X_test)
    rf_proba = rf_model.predict_proba(X_test)

    combined_proba = (nb_proba * Config.NAIVE_BAYES_WEIGHT) + (rf_proba * Config.RANDOM_FOREST_WEIGHT)
    y_pred = np.argmax(combined_proba, axis=1)

    metrics = _compute_metrics(y_test, y_pred, labels, class_names)

    print("\n### Ensemble (Naive Bayes + Random Forest) ###")
    _print_metrics(metrics)
    print(classification_report(
        y_test, y_pred, labels=labels, target_names=class_names, zero_division=0
    ))

    _print_pass_fail(metrics)
    return metrics


def _print_metrics(metrics: dict) -> None:
    print(f"Acurácia:  {metrics['accuracy']:.4f} (meta: >{TARGET_METRICS['accuracy']})")
    print(f"Precisão:  {metrics['precision']:.4f} (meta: >{TARGET_METRICS['precision']})")
    print(f"Recall:    {metrics['recall']:.4f} (meta: >{TARGET_METRICS['recall']})")
    print(f"F1-Score:  {metrics['f1']:.4f} (meta: >{TARGET_METRICS['f1']})")


def _print_pass_fail(metrics: dict) -> None:
    names = {"accuracy": "Acurácia", "precision": "Precisão", "recall": "Recall", "f1": "F1-Score"}
    for key, label in names.items():
        status = "[PASSOU]" if metrics["meets_targets"][key] else "[FALHOU]"
        print(f"{status} - {label}: {metrics[key]:.4f} vs meta {TARGET_METRICS[key]}")


def _build_report(df, dataset_path: Path, class_names: list[str], results: dict) -> dict:
    label_counts = {name: int((df["label"] == name).sum()) for name in class_names}
    return {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "sklearn_version": sklearn.__version__,
        "dataset": {
            "path": str(dataset_path),
            "total_records": int(len(df)),
            "label_counts": label_counts,
            "test_size": Config.TEST_SIZE,
            "cv_folds": Config.CV_FOLDS,
            "random_state": Config.RANDOM_STATE,
        },
        "ensemble_weights": {
            "naive_bayes": Config.NAIVE_BAYES_WEIGHT,
            "random_forest": Config.RANDOM_FOREST_WEIGHT,
        },
        "targets": TARGET_METRICS,
        "results": results,
    }


def _write_metrics_json(report: dict) -> None:
    Config.MODELS_DIR.mkdir(parents=True, exist_ok=True)
    path = Config.MODELS_DIR / "metrics.json"
    path.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"\nMétricas gravadas em: {path}")


def _write_metrics_md(path: Path, report: dict) -> None:
    ds = report["dataset"]
    lines: list[str] = []
    lines.append("# Relatório de métricas — classificação de phishing")
    lines.append("")
    lines.append(
        f"Gerado em {report['generated_at']} · scikit-learn {report['sklearn_version']}"
    )
    lines.append("")
    lines.append(
        f"Dataset: `{ds['path']}` · {ds['total_records']} registros · "
        f"split de teste {ds['test_size']:.0%} · validação cruzada k={ds['cv_folds']}"
    )
    lines.append("")

    lines.append("## Distribuição de classes")
    lines.append("")
    lines.append("| Classe | Registros |")
    lines.append("|---|---|")
    for name, count in ds["label_counts"].items():
        lines.append(f"| {name} | {count} |")
    lines.append("")

    lines.append("## Resultados (média ponderada)")
    lines.append("")
    lines.append("| Modelo | Acurácia | Precisão | Recall | F1 |")
    lines.append("|---|---|---|---|---|")
    pretty = {
        "naive_bayes": "Naive Bayes",
        "random_forest": "Random Forest",
        "ensemble": "Ensemble (NB + RF)",
    }
    for key, label in pretty.items():
        m = report["results"][key]
        lines.append(
            f"| {label} | {m['accuracy']:.4f} | {m['precision']:.4f} | "
            f"{m['recall']:.4f} | {m['f1']:.4f} |"
        )
    lines.append("")
    t = report["targets"]
    lines.append(
        f"Metas do projeto: acurácia > {t['accuracy']}, precisão > {t['precision']}, "
        f"recall > {t['recall']}, F1 > {t['f1']}."
    )
    lines.append("")

    ensemble = report["results"]["ensemble"]
    class_names = list(ensemble["per_class"].keys())

    lines.append("## Matriz de confusão — Ensemble")
    lines.append("")
    lines.append("Linhas = classe verdadeira, colunas = classe prevista.")
    lines.append("")
    lines.append("| verdadeiro \\ previsto | " + " | ".join(class_names) + " |")
    lines.append("|---|" + "|".join(["---"] * len(class_names)) + "|")
    for i, row in enumerate(ensemble["confusion_matrix"]):
        lines.append(f"| {class_names[i]} | " + " | ".join(str(v) for v in row) + " |")
    lines.append("")

    lines.append("## Métricas por classe — Ensemble")
    lines.append("")
    lines.append("| Classe | Precisão | Recall | F1 | Suporte |")
    lines.append("|---|---|---|---|---|")
    for name, m in ensemble["per_class"].items():
        lines.append(
            f"| {name} | {m['precision']:.4f} | {m['recall']:.4f} | "
            f"{m['f1']:.4f} | {m['support']} |"
        )
    lines.append("")

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")
    print(f"Relatório Markdown gravado em: {path}")


def main() -> None:
    # Console Windows costuma usar cp1252; garante que acentos no relatório não quebrem.
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

    parser = argparse.ArgumentParser(description="Avalia modelos de classificação de phishing")
    parser.add_argument(
        "--dataset",
        type=str,
        default=str(Config.DATA_DIR / "dataset.csv"),
        help="Caminho para o CSV do dataset de avaliação",
    )
    parser.add_argument(
        "--report-md",
        type=str,
        default=str(Config.MODELS_DIR / "METRICS.md"),
        help="Caminho do relatório Markdown gerado (use '' para não gerar)",
    )
    args = parser.parse_args()
    report_md = Path(args.report_md) if args.report_md else None
    evaluate(Path(args.dataset), report_md=report_md)


if __name__ == "__main__":
    main()
