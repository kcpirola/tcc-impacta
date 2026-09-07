"""
Configurações centralizadas do microserviço de Machine Learning.
Carrega valores de variáveis de ambiente com defaults seguros para desenvolvimento.
"""
import os
from pathlib import Path

BASE_DIR = Path(__file__).resolve().parent


class Config:
    """Configuração base da aplicação Flask."""

    # Diretórios
    MODELS_DIR = BASE_DIR / "models"
    DATA_DIR = BASE_DIR / "data"

    # Arquivos de modelo
    NAIVE_BAYES_MODEL_PATH = MODELS_DIR / "naive_bayes_model.pkl"
    RANDOM_FOREST_MODEL_PATH = MODELS_DIR / "random_forest_model.pkl"
    TFIDF_VECTORIZER_PATH = MODELS_DIR / "tfidf_vectorizer.pkl"
    FEATURE_SCALER_PATH = MODELS_DIR / "feature_scaler.pkl"

    # Servidor Flask
    HOST = os.getenv("ML_SERVICE_HOST", "0.0.0.0")
    PORT = int(os.getenv("ML_SERVICE_PORT", "5000"))
    DEBUG = os.getenv("ML_SERVICE_DEBUG", "false").lower() == "true"

    # Classificação
    # Pesos do ensemble entre Naive Bayes e Random Forest. A classificação final é o
    # argmax da probabilidade ponderada (sem override por limiar de %).
    NAIVE_BAYES_WEIGHT = float(os.getenv("NAIVE_BAYES_WEIGHT", "0.35"))
    RANDOM_FOREST_WEIGHT = float(os.getenv("RANDOM_FOREST_WEIGHT", "0.65"))

    # Treinamento
    # Naive Bayes e Random Forest usam os parâmetros padrão do scikit-learn
    # (apenas RANDOM_STATE é fixado, para reprodutibilidade — metodologia 2.4.3).
    RANDOM_STATE = int(os.getenv("ML_RANDOM_STATE", "42"))
    TEST_SIZE = float(os.getenv("ML_TEST_SIZE", "0.2"))
    CV_FOLDS = int(os.getenv("ML_CV_FOLDS", "10"))
    TFIDF_MAX_FEATURES = int(os.getenv("TFIDF_MAX_FEATURES", "5000"))
    # Mínimo de amostras por classe (safe/suspicious/phishing) para permitir o treino.
    # Abaixo disso o k-fold(k=10) não é confiável e o treino é abortado.
    MIN_SAMPLES_PER_CLASS = int(os.getenv("ML_MIN_SAMPLES_PER_CLASS", "50"))

    @classmethod
    def ensure_directories(cls):
        """Garante que os diretórios necessários existam."""
        cls.MODELS_DIR.mkdir(parents=True, exist_ok=True)
        cls.DATA_DIR.mkdir(parents=True, exist_ok=True)
