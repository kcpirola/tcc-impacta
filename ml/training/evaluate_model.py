"""
Script de avaliação dos modelos treinados.
Calcula acurácia, precisão, recall e F1-score, comparando com as metas do projeto
(acurácia > 95%, precisão > 94%, recall > 96%, F1 > 95%).

Uso:
    python -m training.evaluate_model --dataset data/dataset.csv
"""
import argparse
import sys
from pathlib import Path

import joblib
import numpy as np
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


def evaluate(dataset_path: Path) -> None:
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

    print("\n" + "=" * 60)
    _evaluate_model(nb_model, X_test, y_test, label_encoder, "Naive Bayes")
    print("\n" + "=" * 60)
    _evaluate_model(rf_model, X_test, y_test, label_encoder, "Random Forest")

    print("\n" + "=" * 60)
    _evaluate_ensemble(nb_model, rf_model, X_test, y_test, label_encoder)


def _evaluate_model(model, X_test, y_test, label_encoder, model_name: str) -> None:
    y_pred = model.predict(X_test)
    precision, recall, f1, _ = precision_recall_fscore_support(y_test, y_pred, average="weighted")
    accuracy = np.mean(y_pred == y_test)

    print(f"\n### {model_name} ###")
    print(f"Acurácia:  {accuracy:.4f} (meta: >{TARGET_METRICS['accuracy']})")
    print(f"Precisão:  {precision:.4f} (meta: >{TARGET_METRICS['precision']})")
    print(f"Recall:    {recall:.4f} (meta: >{TARGET_METRICS['recall']})")
    print(f"F1-Score:  {f1:.4f} (meta: >{TARGET_METRICS['f1']})")
    print("\nRelatório detalhado por classe:")
    print(classification_report(y_test, y_pred, target_names=label_encoder.classes_))
    print("Matriz de confusão:")
    print(confusion_matrix(y_test, y_pred))

    _print_pass_fail(accuracy, precision, recall, f1)


def _evaluate_ensemble(nb_model, rf_model, X_test, y_test, label_encoder) -> None:
    """Avalia o ensemble ponderado (mesma lógica usada em produção)."""
    nb_proba = nb_model.predict_proba(X_test)
    rf_proba = rf_model.predict_proba(X_test)

    combined_proba = (nb_proba * Config.NAIVE_BAYES_WEIGHT) + (rf_proba * Config.RANDOM_FOREST_WEIGHT)
    y_pred = np.argmax(combined_proba, axis=1)

    precision, recall, f1, _ = precision_recall_fscore_support(y_test, y_pred, average="weighted")
    accuracy = np.mean(y_pred == y_test)

    print("\n### Ensemble (Naive Bayes + Random Forest) ###")
    print(f"Acurácia:  {accuracy:.4f} (meta: >{TARGET_METRICS['accuracy']})")
    print(f"Precisão:  {precision:.4f} (meta: >{TARGET_METRICS['precision']})")
    print(f"Recall:    {recall:.4f} (meta: >{TARGET_METRICS['recall']})")
    print(f"F1-Score:  {f1:.4f} (meta: >{TARGET_METRICS['f1']})")
    print(classification_report(y_test, y_pred, target_names=label_encoder.classes_))

    _print_pass_fail(accuracy, precision, recall, f1)


def _print_pass_fail(accuracy, precision, recall, f1) -> None:
    checks = [
        ("Acurácia", accuracy, TARGET_METRICS["accuracy"]),
        ("Precisão", precision, TARGET_METRICS["precision"]),
        ("Recall", recall, TARGET_METRICS["recall"]),
        ("F1-Score", f1, TARGET_METRICS["f1"]),
    ]
    for name, value, target in checks:
        status = "✅ PASSOU" if value >= target else "❌ FALHOU"
        print(f"{status} - {name}: {value:.4f} vs meta {target}")


def main() -> None:
    parser = argparse.ArgumentParser(description="Avalia modelos de classificação de phishing")
    parser.add_argument(
        "--dataset",
        type=str,
        default=str(Config.DATA_DIR / "dataset.csv"),
        help="Caminho para o CSV do dataset de avaliação",
    )
    args = parser.parse_args()
    evaluate(Path(args.dataset))


if __name__ == "__main__":
    main()
