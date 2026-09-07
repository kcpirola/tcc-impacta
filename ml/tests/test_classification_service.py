"""Testes para EmailClassificationService (carregamento e inferência do ensemble)."""
import joblib
import numpy as np
import pytest
from scipy.sparse import csr_matrix, hstack
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.preprocessing import LabelEncoder, MinMaxScaler

from config import Config
from service.classification_service import EmailClassificationService, ModelNotFoundError
from training.data_preprocessing import EmailPreprocessor
from training.feature_extraction import NUMERIC_FEATURE_NAMES, PhishingFeatureExtractor

TRAIN_SAMPLES = [
    ("Reunião de equipe amanhã", "Confirmando nossa reunião às 10h na sala 3.", "joao@empresa.com", "safe"),
    ("Relatório mensal", "Segue em anexo o relatório de vendas do mês.", "maria@empresa.com", "safe"),
    ("Agenda da semana", "Segue a agenda de reuniões desta semana.", "secretaria@empresa.com", "safe"),
    ("Confirme seus dados", "Por favor confirme seus dados de acesso no link.", "suporte@parceiro.com", "suspicious"),
    ("Fatura em anexo", "Segue fatura, favor confirmar pagamento amanhã.", "financeiro@fornecedor.com", "suspicious"),
    ("URGENTE: conta suspensa", "Clique aqui: http://192.168.10.5/login-bank", "alert@bank-alert.tk", "phishing"),
    ("Verifique sua senha agora", "Sua senha expira hoje! http://bit.ly/verify", "no-reply@verify.xyz", "phishing"),
]


@pytest.fixture
def trained_service(tmp_path, monkeypatch):
    """Treina artefatos mínimos e os disponibiliza via Config, retornando um serviço pronto."""
    monkeypatch.setattr(Config, "MODELS_DIR", tmp_path)
    monkeypatch.setattr(Config, "NAIVE_BAYES_MODEL_PATH", tmp_path / "naive_bayes_model.pkl")
    monkeypatch.setattr(Config, "RANDOM_FOREST_MODEL_PATH", tmp_path / "random_forest_model.pkl")
    monkeypatch.setattr(Config, "TFIDF_VECTORIZER_PATH", tmp_path / "tfidf_vectorizer.pkl")
    monkeypatch.setattr(Config, "FEATURE_SCALER_PATH", tmp_path / "feature_scaler.pkl")

    preprocessor = EmailPreprocessor()
    extractor = PhishingFeatureExtractor()

    cleaned_texts = []
    numeric_rows = []
    labels = []
    for subject, body, sender, label in TRAIN_SAMPLES:
        cleaned_texts.append(preprocessor.clean(subject, body))
        numeric_rows.append(extractor.extract(subject, body, sender).as_vector(NUMERIC_FEATURE_NAMES))
        labels.append(label)

    label_encoder = LabelEncoder().fit(["safe", "suspicious", "phishing"])
    y = label_encoder.transform(labels)

    vectorizer = TfidfVectorizer(max_features=200, ngram_range=(1, 2)).fit(cleaned_texts)
    tfidf_matrix = vectorizer.transform(cleaned_texts)

    scaler = MinMaxScaler().fit(np.array(numeric_rows))
    scaled_numeric = scaler.transform(np.array(numeric_rows))

    combined = hstack([tfidf_matrix, csr_matrix(scaled_numeric)]).tocsr()

    nb_model = MultinomialNB().fit(combined, y)
    rf_model = RandomForestClassifier(n_estimators=20, random_state=42).fit(combined, y)

    joblib.dump(nb_model, Config.NAIVE_BAYES_MODEL_PATH)
    joblib.dump(rf_model, Config.RANDOM_FOREST_MODEL_PATH)
    joblib.dump(vectorizer, Config.TFIDF_VECTORIZER_PATH)
    joblib.dump(scaler, Config.FEATURE_SCALER_PATH)
    joblib.dump(label_encoder, tmp_path / "label_encoder.pkl")

    service = EmailClassificationService()
    service.load_models()
    return service


class TestEmailClassificationService:
    def test_is_ready_false_before_loading(self):
        service = EmailClassificationService()
        assert service.is_ready is False

    def test_load_models_raises_when_files_missing(self, tmp_path, monkeypatch):
        monkeypatch.setattr(Config, "MODELS_DIR", tmp_path)
        monkeypatch.setattr(Config, "NAIVE_BAYES_MODEL_PATH", tmp_path / "missing.pkl")
        monkeypatch.setattr(Config, "RANDOM_FOREST_MODEL_PATH", tmp_path / "missing2.pkl")
        monkeypatch.setattr(Config, "TFIDF_VECTORIZER_PATH", tmp_path / "missing3.pkl")
        monkeypatch.setattr(Config, "FEATURE_SCALER_PATH", tmp_path / "missing4.pkl")

        service = EmailClassificationService()
        with pytest.raises(ModelNotFoundError):
            service.load_models()

    def test_classify_raises_when_models_not_loaded(self):
        service = EmailClassificationService()
        with pytest.raises(ModelNotFoundError):
            service.classify("Assunto", "Corpo", "sender@empresa.com")

    def test_classify_phishing_email_returns_phishing_or_suspicious(self, trained_service):
        result = trained_service.classify(
            subject="URGENTE: conta suspensa agora",
            body="Clique aqui imediatamente: http://192.168.10.9/login-bank",
            sender="alert@bank-verify.tk",
        )
        assert result["classification"] in ("phishing", "suspicious")
        assert 0 <= result["confidence"] <= 100
        assert "recommendation" in result

    def test_classify_returns_expected_response_shape(self, trained_service):
        result = trained_service.classify(
            subject="Reunião de equipe",
            body="Confirmando nossa reunião às 10h.",
            sender="joao@empresa.com",
        )
        assert set(result.keys()) == {
            "classification", "confidence", "indicators", "suspiciousUrls", "recommendation"
        }
        assert isinstance(result["indicators"], list)
        assert isinstance(result["suspiciousUrls"], list)

    def test_threshold_override_was_removed(self):
        assert not hasattr(EmailClassificationService, "_apply_threshold_override")

    def test_classification_is_pure_argmax_of_weighted_ensemble(self, trained_service):
        """A classe final é sempre o argmax da proba ponderada — sem override de limiar."""
        cases = [
            ("Confirme seus dados", "Por favor confirme seus dados de acesso no link.", "suporte@parceiro.com"),
            ("URGENTE: conta suspensa", "Clique aqui: http://192.168.10.5/login-bank", "alert@bank-alert.tk"),
            ("Reunião de equipe", "Confirmando nossa reunião às 10h.", "joao@empresa.com"),
        ]
        for subject, body, sender in cases:
            cleaned = trained_service._preprocessor.clean(subject, body)
            extraction = trained_service._extractor.extract(subject, body, sender)
            tfidf = trained_service._vectorizer.transform([cleaned])
            numeric = np.array([extraction.as_vector(NUMERIC_FEATURE_NAMES)])
            scaled = trained_service._scaler.transform(numeric)
            combined = hstack([tfidf, csr_matrix(scaled)]).tocsr()

            nb = trained_service._nb_model.predict_proba(combined)[0]
            rf = trained_service._rf_model.predict_proba(combined)[0]
            weighted = nb * Config.NAIVE_BAYES_WEIGHT + rf * Config.RANDOM_FOREST_WEIGHT
            expected = trained_service._label_encoder.classes_[int(np.argmax(weighted))]

            result = trained_service.classify(subject, body, sender)
            assert result["classification"] == expected
