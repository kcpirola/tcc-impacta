"""
Testes para training.evaluate_model.

Cobrem principalmente a regressão do bug em que `classification_report` /
`precision_recall_fscore_support` quebravam quando o split de teste (20%) não continha
todas as três classes, além da geração de `metrics.json` e `METRICS.md`.
"""
import json

import joblib
import pandas as pd
import pytest
from scipy.sparse import csr_matrix, hstack
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.preprocessing import LabelEncoder, MinMaxScaler

from config import Config
from training.data_preprocessing import EmailPreprocessor
from training.evaluate_model import evaluate
from training.feature_extraction import PhishingFeatureExtractor
from training.train_model import LABEL_ORDER, build_features, load_dataset


@pytest.fixture
def trained_models(tmp_path, monkeypatch):
    """
    Treina artefatos mínimos num dataset propositalmente desbalanceado (phishing raro),
    de forma que o split de teste 80/20 fique sem a classe 'phishing'. Disponibiliza os
    .pkl via Config e devolve (caminho_do_csv, diretório_dos_modelos).
    """
    monkeypatch.setattr(Config, "MODELS_DIR", tmp_path)
    monkeypatch.setattr(Config, "NAIVE_BAYES_MODEL_PATH", tmp_path / "naive_bayes_model.pkl")
    monkeypatch.setattr(Config, "RANDOM_FOREST_MODEL_PATH", tmp_path / "random_forest_model.pkl")
    monkeypatch.setattr(Config, "TFIDF_VECTORIZER_PATH", tmp_path / "tfidf_vectorizer.pkl")
    monkeypatch.setattr(Config, "FEATURE_SCALER_PATH", tmp_path / "feature_scaler.pkl")

    rows = []
    for i in range(12):
        rows.append((f"Reuniao de equipe {i}", "Confirmando nossa reuniao as 10h na sala 3.", "colega@empresa.com", "safe"))
    for i in range(12):
        rows.append((f"Confirme seus dados {i}", "Por favor confirme seus dados de acesso no link enviado.", "suporte@parceiro.com", "suspicious"))
    for i in range(2):
        rows.append((f"URGENTE conta suspensa {i}", "Clique aqui imediatamente: http://192.168.10.5/login-bank", "alert@bank-alert.tk", "phishing"))

    csv_path = tmp_path / "dataset.csv"
    pd.DataFrame(rows, columns=["subject", "body", "sender", "label"]).to_csv(csv_path, index=False)

    df = load_dataset(csv_path)
    cleaned_texts, numeric_matrix = build_features(df, EmailPreprocessor(), PhishingFeatureExtractor())

    label_encoder = LabelEncoder().fit(LABEL_ORDER)
    y = label_encoder.transform(df["label"])

    vectorizer = TfidfVectorizer(max_features=200, ngram_range=(1, 2)).fit(cleaned_texts)
    tfidf_matrix = vectorizer.transform(cleaned_texts)
    scaler = MinMaxScaler().fit(numeric_matrix)
    scaled_numeric = scaler.transform(numeric_matrix)
    combined = hstack([tfidf_matrix, csr_matrix(scaled_numeric)]).tocsr()

    nb_model = MultinomialNB().fit(combined, y)
    rf_model = RandomForestClassifier(n_estimators=20, random_state=42).fit(combined, y)

    joblib.dump(nb_model, Config.NAIVE_BAYES_MODEL_PATH)
    joblib.dump(rf_model, Config.RANDOM_FOREST_MODEL_PATH)
    joblib.dump(vectorizer, Config.TFIDF_VECTORIZER_PATH)
    joblib.dump(scaler, Config.FEATURE_SCALER_PATH)
    joblib.dump(label_encoder, tmp_path / "label_encoder.pkl")

    return csv_path, tmp_path


class TestEvaluate:
    def test_runs_when_test_split_misses_a_class(self, trained_models):
        csv_path, models_dir = trained_models

        # Não deve lançar (era o bug: ValueError sobre número de classes vs target_names).
        report = evaluate(csv_path, report_md=models_dir / "METRICS.md")

        assert set(report["results"]) == {"naive_bayes", "random_forest", "ensemble"}
        for name, metrics in report["results"].items():
            assert 0.0 <= metrics["accuracy"] <= 1.0
            assert 0.0 <= metrics["f1"] <= 1.0
            assert len(metrics["confusion_matrix"]) == 3
            assert all(len(row) == 3 for row in metrics["confusion_matrix"])
            assert set(metrics["per_class"]) == {"safe", "suspicious", "phishing"}

    def test_writes_metrics_json_and_markdown(self, trained_models):
        csv_path, models_dir = trained_models

        evaluate(csv_path, report_md=models_dir / "METRICS.md")

        metrics_json = models_dir / "metrics.json"
        metrics_md = models_dir / "METRICS.md"
        assert metrics_json.exists()
        assert metrics_md.exists()

        data = json.loads(metrics_json.read_text(encoding="utf-8"))
        assert "generated_at" in data
        assert "sklearn_version" in data
        assert data["dataset"]["label_counts"] == {"phishing": 2, "safe": 12, "suspicious": 12}
        assert data["dataset"]["cv_folds"] == Config.CV_FOLDS
        assert "ensemble" in data["results"]
        assert data["results"]["ensemble"]["meets_targets"].keys() == {
            "accuracy", "precision", "recall", "f1"
        }

        md = metrics_md.read_text(encoding="utf-8")
        assert "Relatório de métricas" in md
        assert "Ensemble" in md

    def test_report_md_can_be_disabled(self, trained_models):
        csv_path, models_dir = trained_models

        evaluate(csv_path, report_md=None)

        assert (models_dir / "metrics.json").exists()
        assert not (models_dir / "METRICS.md").exists()
