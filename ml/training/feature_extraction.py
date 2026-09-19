"""
Módulo de extração de features heurísticas de e-mails.
Complementa as features textuais (TF-IDF) com sinais estruturais de phishing.
"""
import re
from dataclasses import dataclass, field

import tldextract

from training.data_preprocessing import EmailPreprocessor

URGENT_KEYWORDS = [
    "urgent", "urgente", "immediately", "imediatamente", "verify your account",
    "verifique sua conta", "suspended", "suspenso", "click here", "clique aqui",
    "act now", "aja agora", "confirm your", "confirme sua", "limited time",
    "tempo limitado", "password expires", "senha expira", "unusual activity",
    "atividade incomum",
]

SUSPICIOUS_TLDS = {"tk", "ml", "ga", "cf", "gq", "xyz", "top", "club", "work"}

IP_URL_PATTERN = re.compile(r"https?://\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")
SHORTENER_DOMAINS = {"bit.ly", "tinyurl.com", "goo.gl", "t.co", "ow.ly", "is.gd"}


def _plural(quantidade: int, singular: str, plural: str) -> str:
    """Escolhe a forma singular ou plural conforme a quantidade."""
    return singular if quantidade == 1 else plural


@dataclass
class PhishingIndicatorResult:
    """Representa um indicador de phishing identificado, com seu score de risco."""

    indicator_type: str
    description: str
    risk_score: float


@dataclass
class FeatureExtractionResult:
    """Agrupa o vetor de features numéricas e os indicadores explicáveis encontrados."""

    numeric_features: dict[str, float] = field(default_factory=dict)
    indicators: list[PhishingIndicatorResult] = field(default_factory=list)
    suspicious_urls: list[str] = field(default_factory=list)

    def as_vector(self, feature_order: list[str]) -> list[float]:
        """Converte as features numéricas em um vetor ordenado, para uso pelo modelo."""
        return [self.numeric_features.get(name, 0.0) for name in feature_order]


# Ordem fixa das features numéricas, usada tanto no treino quanto na inferência
NUMERIC_FEATURE_NAMES = [
    "num_urls",
    "num_suspicious_urls",
    "has_ip_url",
    "has_shortened_url",
    "num_urgent_keywords",
    "subject_has_urgent_word",
    "num_exclamation_marks",
    "num_uppercase_words",
    "sender_domain_mismatch",
    "body_length",
    "num_attachments_mentioned",
    "has_generic_greeting",
]


class PhishingFeatureExtractor:
    """
    Extrai features estruturais e heurísticas de um e-mail.
    Segue SRP: apenas identifica sinais de risco, não decide a classificação final.
    """

    def __init__(self) -> None:
        self._preprocessor = EmailPreprocessor()

    def extract(self, subject: str, body: str, sender: str = "") -> FeatureExtractionResult:
        """
        Extrai o conjunto completo de features heurísticas de um e-mail.

        Args:
            subject: Assunto do e-mail.
            body: Corpo do e-mail (HTML ou texto puro).
            sender: Endereço de e-mail do remetente.

        Returns:
            FeatureExtractionResult com features numéricas e indicadores explicáveis.
        """
        result = FeatureExtractionResult()
        raw_text = f"{subject or ''} {body or ''}"
        plain_text = self._preprocessor.strip_html(body or "")
        urls = self._preprocessor.extract_urls(raw_text)

        self._analyze_urls(urls, result)
        self._analyze_urgent_language(subject or "", plain_text, result)
        self._analyze_formatting(plain_text, result)
        self._analyze_sender(sender or "", urls, result)
        self._analyze_greeting(plain_text, result)

        result.numeric_features["body_length"] = float(len(plain_text))
        result.numeric_features["num_attachments_mentioned"] = float(
            len(re.findall(r"attach|anexo", plain_text, re.IGNORECASE))
        )

        return result

    def _analyze_urls(self, urls: list[str], result: FeatureExtractionResult) -> None:
        result.numeric_features["num_urls"] = float(len(urls))

        suspicious_count = 0
        has_ip_url = 0.0
        has_shortened_url = 0.0

        for url in urls:
            is_suspicious = False

            if IP_URL_PATTERN.match(url):
                has_ip_url = 1.0
                is_suspicious = True

            extracted = tldextract.extract(url)
            domain = f"{extracted.domain}.{extracted.suffix}".lower()

            if domain in SHORTENER_DOMAINS:
                has_shortened_url = 1.0
                is_suspicious = True

            if extracted.suffix.lower() in SUSPICIOUS_TLDS:
                is_suspicious = True

            if is_suspicious:
                suspicious_count += 1
                result.suspicious_urls.append(url)

        result.numeric_features["num_suspicious_urls"] = float(suspicious_count)
        result.numeric_features["has_ip_url"] = has_ip_url
        result.numeric_features["has_shortened_url"] = has_shortened_url

        if suspicious_count > 0:
            result.indicators.append(
                PhishingIndicatorResult(
                    indicator_type="suspicious_url",
                    description=(
                        f"{suspicious_count} "
                        f"{_plural(suspicious_count, 'URL', 'URLs')} "
                        f"{_plural(suspicious_count, '\"suspeita\"', '\"suspeitas\"')} "
                        f"{_plural(suspicious_count, 'detectada', 'detectadas')}"
                    ),
                    risk_score=min(100.0, suspicious_count * 25.0),
                )
            )

    def _analyze_urgent_language(self, subject: str, body: str, result: FeatureExtractionResult) -> None:
        text = f"{subject} {body}".lower()
        count = sum(1 for keyword in URGENT_KEYWORDS if keyword in text)
        result.numeric_features["num_urgent_keywords"] = float(count)
        result.numeric_features["subject_has_urgent_word"] = float(
            any(keyword in subject.lower() for keyword in URGENT_KEYWORDS)
        )

        if count > 0:
            result.indicators.append(
                PhishingIndicatorResult(
                    indicator_type="urgent_language",
                    description=(
                        f"{count} "
                        f"{_plural(count, 'termo', 'termos')} "
                        f'de "urgência" '
                        f"{_plural(count, 'detectado', 'detectados')}"
                    ),
                    risk_score=min(100.0, count * 20.0),
                )
            )

    def _analyze_formatting(self, body: str, result: FeatureExtractionResult) -> None:
        result.numeric_features["num_exclamation_marks"] = float(body.count("!"))

        words = body.split()
        uppercase_words = [w for w in words if len(w) > 2 and w.isupper()]
        result.numeric_features["num_uppercase_words"] = float(len(uppercase_words))

    def _analyze_sender(self, sender: str, urls: list[str], result: FeatureExtractionResult) -> None:
        sender_domain = sender.split("@")[-1].lower() if "@" in sender else ""
        mismatch = 0.0

        for url in urls:
            extracted = tldextract.extract(url)
            url_domain = f"{extracted.domain}.{extracted.suffix}".lower()
            if url_domain and sender_domain and url_domain not in sender_domain and sender_domain not in url_domain:
                mismatch = 1.0
                break

        result.numeric_features["sender_domain_mismatch"] = mismatch

        if mismatch:
            result.indicators.append(
                PhishingIndicatorResult(
                    indicator_type="sender_domain_mismatch",
                    description="Domínio do remetente não corresponde às URLs do corpo",
                    risk_score=60.0,
                )
            )

    def _analyze_greeting(self, body: str, result: FeatureExtractionResult) -> None:
        generic_greetings = ["dear customer", "dear user", "caro cliente", "prezado cliente", "dear sir/madam"]
        has_generic = any(greeting in body.lower() for greeting in generic_greetings)
        result.numeric_features["has_generic_greeting"] = float(has_generic)

        if has_generic:
            result.indicators.append(
                PhishingIndicatorResult(
                    indicator_type="generic_greeting",
                    description="Saudação genérica detectada (sem personalização)",
                    risk_score=30.0,
                )
            )
