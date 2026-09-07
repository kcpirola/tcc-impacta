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
    RAW_DATA_DIR = DATA_DIR / "raw"

    # Arquivos de modelo
    NAIVE_BAYES_MODEL_PATH = MODELS_DIR / "naive_bayes_model.pkl"
    RANDOM_FOREST_MODEL_PATH = MODELS_DIR / "random_forest_model.pkl"
    TFIDF_VECTORIZER_PATH = MODELS_DIR / "tfidf_vectorizer.pkl"
    FEATURE_SCALER_PATH = MODELS_DIR / "feature_scaler.pkl"

    # Dataset consolidado (saída de training.build_dataset, entrada de train_model/evaluate)
    PROCESSED_DATASET_PATH = DATA_DIR / "dataset.csv"
    DATASET_STATS_PATH = DATA_DIR / "dataset_stats.json"

    # Fontes de dados (ver ml/data/README.md)
    # Sting9 Research Initiative — API autenticada em sting9.org.
    STING9_API_BASE = os.getenv("STING9_API_BASE", "https://sting9.org/api/v1")
    # JWT obtido no login; STING9_API_KEY é aceito como alias.
    STING9_TOKEN = os.getenv("STING9_TOKEN", "") or os.getenv("STING9_API_KEY", "")
    # Caminho do export JSON já baixado; vazio = usa o data/raw/sting9_export_*.json mais recente.
    STING9_EXPORT_PATH = os.getenv("STING9_EXPORT_PATH", "")
    # Phishing Email Dataset (Kaggle) — slug owner/dataset para download via CLI.
    KAGGLE_DATASET_SLUG = os.getenv("KAGGLE_DATASET_SLUG", "")

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
        cls.RAW_DATA_DIR.mkdir(parents=True, exist_ok=True)
