"""Testes de integração para a API Flask do microserviço de ML."""
from unittest.mock import patch

import pytest

from service import app as app_module


@pytest.fixture
def client():
    app_module.app.config.update(TESTING=True)
    with app_module.app.test_client() as test_client:
        yield test_client


class TestHealthEndpoint:
    def test_health_returns_503_when_models_not_loaded(self, client):
        with patch.object(app_module.classification_service, "_loaded", False):
            response = client.get("/health")
            assert response.status_code == 503
            assert response.get_json()["status"] == "models_not_loaded"

    def test_health_returns_200_when_models_loaded(self, client):
        with patch.object(app_module.classification_service, "_loaded", True):
            response = client.get("/health")
            assert response.status_code == 200
            assert response.get_json()["status"] == "healthy"


class TestClassifyEndpoint:
    def test_classify_returns_503_when_models_not_ready(self, client):
        with patch.object(app_module.classification_service, "_loaded", False), \
             patch.object(app_module, "_try_load_models"):
            response = client.post("/classify", json={
                "subject": "Assunto", "body": "Corpo", "sender": "a@empresa.com"
            })
            assert response.status_code == 503

    def test_classify_returns_400_when_body_missing(self, client):
        with patch.object(app_module.classification_service, "_loaded", True):
            response = client.post("/classify", json={})
            assert response.status_code == 400

    def test_classify_returns_400_when_sender_missing(self, client):
        with patch.object(app_module.classification_service, "_loaded", True):
            response = client.post("/classify", json={"subject": "A", "body": "B"})
            assert response.status_code == 400

    def test_classify_returns_result_from_service(self, client):
        fake_result = {
            "classification": "phishing",
            "confidence": 90.0,
            "indicators": [],
            "suspiciousUrls": [],
            "recommendation": "Não clique em links.",
        }
        with patch.object(app_module.classification_service, "_loaded", True), \
             patch.object(app_module.classification_service, "classify", return_value=fake_result):
            response = client.post("/classify", json={
                "subject": "Urgente", "body": "Clique aqui", "sender": "attacker@malicious.tk"
            })
            assert response.status_code == 200
            assert response.get_json()["classification"] == "phishing"


class TestClassifyBatchEndpoint:
    def test_classify_batch_requires_list_payload(self, client):
        with patch.object(app_module.classification_service, "_loaded", True):
            response = client.post("/classify-batch", json={"subject": "A"})
            assert response.status_code == 400

    def test_classify_batch_returns_result_per_email(self, client):
        fake_result = {
            "classification": "safe",
            "confidence": 95.0,
            "indicators": [],
            "suspiciousUrls": [],
            "recommendation": "E-mail seguro.",
        }
        with patch.object(app_module.classification_service, "_loaded", True), \
             patch.object(app_module.classification_service, "classify", return_value=fake_result):
            response = client.post("/classify-batch", json=[
                {"subject": "A", "body": "A", "sender": "a@empresa.com"},
                {"subject": "B", "body": "B", "sender": "b@empresa.com"},
            ])
            assert response.status_code == 200
            body = response.get_json()
            assert len(body) == 2
            assert all(item["classification"] == "safe" for item in body)
