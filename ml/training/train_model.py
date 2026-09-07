"""
Script de treinamento dos modelos de classificação de phishing.
Treina Naive Bayes (triagem inicial) e Random Forest (classificação final)
sobre features de TF-IDF combinadas com indicadores heurísticos.

Uso:
    python -m training.train_model --dataset data/dataset.csv
"""
import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

import joblib
import numpy as np
import pandas as pd
from scipy.sparse import hstack, csr_matrix
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.model_selection import StratifiedKFold, cross_val_score, train_test_split
from sklearn.naive_bayes import MultinomialNB
from sklearn.preprocessing import MinMaxScaler, LabelEncoder

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config
from training.data_preprocessing import EmailPreprocessor
from training.feature_extraction import NUMERIC_FEATURE_NAMES, PhishingFeatureExtractor

LABEL_ORDER = ["safe", "suspicious", "phishing"]


def load_dataset(csv_path: Path) -> pd.DataFrame:
    """
    Carrega o dataset rotulado de e-mails.

    Espera um CSV com colunas: subject, body, sender, label.
    label deve ser um de: safe, suspicious, phishing.
    """
    df = pd.read_csv(csv_path)
    required_columns = {"subject", "body", "sender", "label"}
    missing = required_columns - set(df.columns)
    if missing:
        raise ValueError(f"Dataset está faltando as colunas: {missing}")

    df["label"] = df["label"].str.lower().str.strip()
    invalid_labels = set(df["label"].unique()) - set(LABEL_ORDER)
    if invalid_labels:
        raise ValueError(f"Rótulos inválidos encontrados: {invalid_labels}. Use: {LABEL_ORDER}")

    return df.fillna("")


def build_features(df: pd.DataFrame, preprocessor: EmailPreprocessor, extractor: PhishingFeatureExtractor):
    """Constrói o texto limpo e a matriz de features heurísticas para todo o dataset."""
    cleaned_texts = []
    numeric_rows = []

    for _, row in df.iterrows():
        cleaned_texts.append(preprocessor.clean(row["subject"], row["body"]))
        features = extractor.extract(row["subject"], row["body"], row["sender"])
        numeric_rows.append(features.as_vector(NUMERIC_FEATURE_NAMES))

    numeric_matrix = np.array(numeric_rows, dtype=float)
    return cleaned_texts, numeric_matrix


def _resolve_cv_folds(label_counts: pd.Series) -> int:
    """
    Valida o dataset e resolve o número de folds da validação cruzada.

    - Aborta se alguma das três classes estiver ausente ou com menos de
      Config.MIN_SAMPLES_PER_CLASS amostras (k-fold não seria confiável).
    - Caso contrário usa k = min(Config.CV_FOLDS, menor classe), reduzindo k apenas
      se a menor classe tiver menos amostras que o k configurado.
    """
    present = set(label_counts.index)
    missing = set(LABEL_ORDER) - present
    if missing:
        raise ValueError(
            f"Dataset não contém as classes: {sorted(missing)}. "
            f"Gere um dataset real com `python -m training.build_dataset` antes de treinar."
        )

    min_count = int(label_counts.min())
    if min_count < Config.MIN_SAMPLES_PER_CLASS:
        smallest = label_counts.idxmin()
        raise ValueError(
            f"Classe menos populada ('{smallest}') tem {min_count} amostra(s); o mínimo "
            f"para treino confiável é {Config.MIN_SAMPLES_PER_CLASS} por classe. "
            f"Gere um dataset real com `python -m training.build_dataset` antes de treinar."
        )

    k = min(Config.CV_FOLDS, min_count)
    if k < Config.CV_FOLDS:
        print(
            f"AVISO: reduzindo k de {Config.CV_FOLDS} para {k} "
            f"(classe menos populada tem {min_count} amostras)."
        )
    return k


def train(dataset_path: Path) -> None:
    Config.ensure_directories()

    print(f"Carregando dataset de {dataset_path}...")
    df = load_dataset(dataset_path)
    print(f"Total de registros: {len(df)}")
    label_counts = df["label"].value_counts()
    print(label_counts)

    cv_folds = _resolve_cv_folds(label_counts)

    preprocessor = EmailPreprocessor()
    extractor = PhishingFeatureExtractor()

    print("Extraindo features de texto e heurísticas...")
    cleaned_texts, numeric_matrix = build_features(df, preprocessor, extractor)

    label_encoder = LabelEncoder()
    label_encoder.fit(LABEL_ORDER)
    y = label_encoder.transform(df["label"])

    # Vetorização TF-IDF do texto limpo
    vectorizer = TfidfVectorizer(max_features=Config.TFIDF_MAX_FEATURES, ngram_range=(1, 2))
    tfidf_matrix = vectorizer.fit_transform(cleaned_texts)

    # Normaliza features heurísticas para a mesma escala do TF-IDF
    scaler = MinMaxScaler()
    scaled_numeric = scaler.fit_transform(numeric_matrix)

    # Combina features textuais (esparsas) com features heurísticas (densas)
    combined_features = hstack([tfidf_matrix, csr_matrix(scaled_numeric)]).tocsr()

    X_train, X_test, y_train, y_test = train_test_split(
        combined_features, y,
        test_size=Config.TEST_SIZE,
        random_state=Config.RANDOM_STATE,
        stratify=y,
    )

    # Naive Bayes exige valores não-negativos; TF-IDF + MinMax garantem isso.
    # Parâmetros padrão do scikit-learn (metodologia 2.4.3).
    print("\nTreinando Naive Bayes...")
    nb_model = MultinomialNB()
    nb_model.fit(X_train, y_train)
    nb_cv = _cross_validate(nb_model, combined_features, y, "Naive Bayes", cv_folds)

    print("\nTreinando Random Forest...")
    rf_model = RandomForestClassifier(random_state=Config.RANDOM_STATE, n_jobs=-1)
    rf_model.fit(X_train, y_train)
    rf_cv = _cross_validate(rf_model, combined_features, y, "Random Forest", cv_folds)

    print("\nSalvando modelos treinados...")
    joblib.dump(nb_model, Config.NAIVE_BAYES_MODEL_PATH)
    joblib.dump(rf_model, Config.RANDOM_FOREST_MODEL_PATH)
    joblib.dump(vectorizer, Config.TFIDF_VECTORIZER_PATH)
    joblib.dump(scaler, Config.FEATURE_SCALER_PATH)
    joblib.dump(label_encoder, Config.MODELS_DIR / "label_encoder.pkl")

    _write_cv_metrics(cv_folds, {"naive_bayes": nb_cv, "random_forest": rf_cv})

    print(f"Modelos salvos em: {Config.MODELS_DIR}")
    print("\nTreinamento concluído. Execute evaluate_model.py para métricas detalhadas.")


def _cross_validate(model, X, y, model_name: str, n_splits: int) -> dict:
    """Executa validação cruzada k-fold, imprime e retorna a acurácia por fold."""
    cv = StratifiedKFold(n_splits=n_splits, shuffle=True, random_state=Config.RANDOM_STATE)
    scores = cross_val_score(model, X, y, cv=cv, scoring="accuracy")
    print(f"{model_name} - Acurácia CV (k={n_splits}): {scores.mean():.4f} (+/- {scores.std():.4f})")
    return {
        "mean": float(scores.mean()),
        "std": float(scores.std()),
        "scores": [float(s) for s in scores],
    }


def _write_cv_metrics(n_splits: int, results: dict) -> None:
    """Persiste as métricas de validação cruzada em models/cv_metrics.json."""
    Config.MODELS_DIR.mkdir(parents=True, exist_ok=True)
    payload = {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "cv_folds": n_splits,
        "scoring": "accuracy",
        **results,
    }
    path = Config.MODELS_DIR / "cv_metrics.json"
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Métricas de validação cruzada gravadas em: {path}")


def main() -> None:
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

    parser = argparse.ArgumentParser(description="Treina modelos de classificação de phishing")
    parser.add_argument(
        "--dataset",
        type=str,
        default=str(Config.DATA_DIR / "dataset.csv"),
        help="Caminho para o CSV do dataset (colunas: subject, body, sender, label)",
    )
    args = parser.parse_args()
    try:
        train(Path(args.dataset))
    except ValueError as exc:
        print(f"\nErro: {exc}", file=sys.stderr)
        raise SystemExit(1)


if __name__ == "__main__":
    main()
