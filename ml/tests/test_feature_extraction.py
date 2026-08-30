"""Testes unitários para PhishingFeatureExtractor (indicadores heurísticos)."""
from training.feature_extraction import NUMERIC_FEATURE_NAMES, PhishingFeatureExtractor


class TestPhishingFeatureExtractor:
    def setup_method(self):
        self.extractor = PhishingFeatureExtractor()

    def test_extract_detects_ip_based_url_as_suspicious(self):
        result = self.extractor.extract(
            subject="Urgente",
            body="Acesse http://192.168.1.1/login agora",
            sender="alert@empresa.com",
        )
        assert result.numeric_features["has_ip_url"] == 1.0
        assert result.numeric_features["num_suspicious_urls"] >= 1.0
        assert len(result.suspicious_urls) == 1

    def test_extract_detects_shortened_url(self):
        result = self.extractor.extract(
            subject="Prêmio",
            body="Resgate aqui: http://bit.ly/premio",
            sender="promo@empresa.com",
        )
        assert result.numeric_features["has_shortened_url"] == 1.0

    def test_extract_detects_urgent_language_in_subject(self):
        result = self.extractor.extract(
            subject="URGENTE: verifique sua conta",
            body="Corpo normal sem termos suspeitos",
            sender="alert@empresa.com",
        )
        assert result.numeric_features["subject_has_urgent_word"] == 1.0
        assert result.numeric_features["num_urgent_keywords"] >= 1.0
        assert any(i.indicator_type == "urgent_language" for i in result.indicators)

    def test_extract_detects_sender_domain_mismatch(self):
        result = self.extractor.extract(
            subject="Confirmação",
            body="Acesse http://outro-dominio.com/confirm",
            sender="contato@empresa-legitima.com",
        )
        assert result.numeric_features["sender_domain_mismatch"] == 1.0

    def test_extract_detects_generic_greeting(self):
        result = self.extractor.extract(
            subject="Aviso",
            body="Dear Customer, please update your information.",
            sender="noreply@empresa.com",
        )
        assert result.numeric_features["has_generic_greeting"] == 1.0

    def test_extract_safe_email_has_low_risk_signals(self):
        result = self.extractor.extract(
            subject="Reunião de equipe amanhã",
            body="Olá, confirmando nossa reunião às 10h na sala 3.",
            sender="joao.silva@empresa.com",
        )
        assert result.numeric_features["num_suspicious_urls"] == 0.0
        assert result.numeric_features["num_urgent_keywords"] == 0.0
        assert result.indicators == []

    def test_as_vector_respects_feature_order(self):
        result = self.extractor.extract("Assunto", "Corpo", "sender@empresa.com")
        vector = result.as_vector(NUMERIC_FEATURE_NAMES)
        assert len(vector) == len(NUMERIC_FEATURE_NAMES)
        assert vector[NUMERIC_FEATURE_NAMES.index("body_length")] == float(len("Corpo"))
