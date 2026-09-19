"""
Serviço de classificação de e-mails.
Carrega os modelos treinados e executa a inferência do ensemble
Naive Bayes + Random Forest, retornando classificação, confiança e indicadores.
"""
import sys
from pathlib import Path

import joblib
import numpy as np
from scipy.sparse import hstack, csr_matrix

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config
from training.data_preprocessing import EmailPreprocessor
from training.feature_extraction import NUMERIC_FEATURE_NAMES, PhishingFeatureExtractor


class ModelNotFoundError(Exception):
    """Lançada quando os modelos treinados não foram encontrados em disco."""


class EmailClassificationService:
    """
    Orquestra a classificação de e-mails combinando Naive Bayes e Random Forest.
    Segue SRP: apenas classifica, não treina nem serve HTTP (isso é do app.py).
    """

    def __init__(self) -> None:
        self._preprocessor = EmailPreprocessor()
        self._extractor = PhishingFeatureExtractor()
        self._nb_model = None
        self._rf_model = None
        self._vectorizer = None
        self._scaler = None
        self._label_encoder = None
        self._loaded = False

    def load_models(self) -> None:
        """Carrega os modelos e artefatos treinados a partir do disco."""
        required_files = [
            Config.NAIVE_BAYES_MODEL_PATH,
            Config.RANDOM_FOREST_MODEL_PATH,
            Config.TFIDF_VECTORIZER_PATH,
            Config.FEATURE_SCALER_PATH,
        ]
        missing = [str(f) for f in required_files if not f.exists()]
        if missing:
            raise ModelNotFoundError(
                f"Modelos não encontrados: {missing}. Execute training/train_model.py primeiro."
            )

        self._nb_model = joblib.load(Config.NAIVE_BAYES_MODEL_PATH)
        self._rf_model = joblib.load(Config.RANDOM_FOREST_MODEL_PATH)
        self._vectorizer = joblib.load(Config.TFIDF_VECTORIZER_PATH)
        self._scaler = joblib.load(Config.FEATURE_SCALER_PATH)
        self._label_encoder = joblib.load(Config.MODELS_DIR / "label_encoder.pkl")
        self._loaded = True

    @property
    def is_ready(self) -> bool:
        """Indica se os modelos já foram carregados e o serviço está pronto para uso."""
        return self._loaded

    def classify(self, subject: str, body: str, sender: str, recipients: str = "") -> dict:
        """
        Classifica um e-mail como safe, suspicious ou phishing.

        Args:
            subject: Assunto do e-mail.
            body: Corpo do e-mail.
            sender: Endereço do remetente.
            recipients: Destinatários (não utilizado na classificação atual).

        Returns:
            Dicionário compatível com o DTO ClassificationResponse do backend .NET.
        """
        if not self._loaded:
            raise ModelNotFoundError("Modelos não carregados. Chame load_models() primeiro.")

        cleaned_text = self._preprocessor.clean(subject, body)
        extraction = self._extractor.extract(subject, body, sender)

        tfidf_vector = self._vectorizer.transform([cleaned_text])
        numeric_vector = np.array([extraction.as_vector(NUMERIC_FEATURE_NAMES)])
        scaled_numeric = self._scaler.transform(numeric_vector)
        combined = hstack([tfidf_vector, csr_matrix(scaled_numeric)]).tocsr()

        nb_proba = self._nb_model.predict_proba(combined)[0]
        rf_proba = self._rf_model.predict_proba(combined)[0]

        combined_proba = (
            nb_proba * Config.NAIVE_BAYES_WEIGHT + rf_proba * Config.RANDOM_FOREST_WEIGHT
        )

        # Decisão final = argmax da probabilidade ponderada do ensemble (sem override
        # por limiar de %), fiel à metodologia ("classificação pelo modelo").
        predicted_index = int(np.argmax(combined_proba))
        classification = self._label_encoder.classes_[predicted_index]
        confidence = round(float(combined_proba[predicted_index]) * 100, 2)

        return {
            "classification": classification,
            "confidence": confidence,
            "indicators": [
                {
                    "type": indicator.indicator_type,
                    "description": indicator.description,
                    "riskScore": indicator.risk_score,
                }
                for indicator in extraction.indicators
            ],
            "suspiciousUrls": extraction.suspicious_urls,
            "recommendation": self._build_recommendation(classification),
        }

    @staticmethod
    def _build_recommendation(classification: str) -> str:
        recommendations = {
            # Sem afirmar "nenhum sinal": o extrator de indicadores roda independentemente
            # do veredito e pode listar sinais (urgência, por exemplo) num e-mail seguro.
            "safe": "Nenhum indício relevante de phishing. Ainda assim, confira o remetente antes de clicar em links.",
            "suspicious": "E-mail suspeito. Revise os indicadores antes de interagir com links ou anexos.",
            "phishing": "Este e-mail é provavelmente phishing. Não clique em links nem forneça dados.",
        }
        return recommendations.get(classification, "Classificação indisponível.")
