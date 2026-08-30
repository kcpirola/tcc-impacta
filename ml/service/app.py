"""
API Flask do microserviço de Machine Learning para detecção de phishing.
Consumida pelo backend .NET via Refit (MlServiceSettings.BaseUrl).

Endpoints:
    GET  /health              - Verifica se o serviço e os modelos estão prontos
    POST /classify            - Classifica um único e-mail
    POST /classify-batch      - Classifica múltiplos e-mails em lote
"""
import logging
import sys
from pathlib import Path

from flask import Flask, jsonify, request
from flask_cors import CORS

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config
from service.classification_service import EmailClassificationService, ModelNotFoundError

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger("phishing_ml_service")

app = Flask(__name__)
CORS(app)

classification_service = EmailClassificationService()


def _try_load_models() -> None:
    try:
        classification_service.load_models()
        logger.info("Modelos carregados com sucesso.")
    except ModelNotFoundError as ex:
        logger.warning("Modelos ainda não disponíveis: %s", ex)


_try_load_models()


@app.route("/health", methods=["GET"])
def health():
    """Verifica a saúde do serviço e se os modelos estão carregados."""
    status = "healthy" if classification_service.is_ready else "models_not_loaded"
    status_code = 200 if classification_service.is_ready else 503
    return jsonify({"status": status, "service": "phishing-ml-service"}), status_code


@app.route("/classify", methods=["POST"])
def classify_email():
    """Classifica um único e-mail como safe, suspicious ou phishing."""
    if not classification_service.is_ready:
        _try_load_models()
        if not classification_service.is_ready:
            return jsonify({"error": "Modelos não carregados. Execute o treinamento primeiro."}), 503

    payload = request.get_json(silent=True)
    if not payload:
        return jsonify({"error": "Corpo da requisição inválido ou ausente"}), 400

    subject = payload.get("subject", "")
    body = payload.get("body", "")
    sender = payload.get("sender", "")
    recipients = payload.get("recipients", "")

    if not sender:
        return jsonify({"error": "Campo 'sender' é obrigatório"}), 400

    try:
        result = classification_service.classify(subject, body, sender, recipients)
        return jsonify(result), 200
    except Exception:
        logger.exception("Erro ao classificar e-mail")
        return jsonify({"error": "Erro interno ao classificar e-mail"}), 500


@app.route("/classify-batch", methods=["POST"])
def classify_emails_batch():
    """Classifica múltiplos e-mails em uma única requisição."""
    if not classification_service.is_ready:
        _try_load_models()
        if not classification_service.is_ready:
            return jsonify({"error": "Modelos não carregados. Execute o treinamento primeiro."}), 503

    payload = request.get_json(silent=True)
    if not payload or not isinstance(payload, list):
        return jsonify({"error": "Corpo da requisição deve ser uma lista de e-mails"}), 400

    results = []
    for item in payload:
        sender = item.get("sender", "")
        if not sender:
            results.append({"error": "Campo 'sender' é obrigatório"})
            continue
        try:
            result = classification_service.classify(
                item.get("subject", ""),
                item.get("body", ""),
                sender,
                item.get("recipients", ""),
            )
            results.append(result)
        except Exception:
            logger.exception("Erro ao classificar e-mail no lote")
            results.append({"error": "Erro ao classificar este e-mail"})

    return jsonify(results), 200


@app.route("/reload-models", methods=["POST"])
def reload_models():
    """Recarrega os modelos do disco (útil após um novo treinamento)."""
    try:
        classification_service.load_models()
        return jsonify({"message": "Modelos recarregados com sucesso"}), 200
    except ModelNotFoundError as ex:
        return jsonify({"error": str(ex)}), 503


if __name__ == "__main__":
    app.run(host=Config.HOST, port=Config.PORT, debug=Config.DEBUG)
