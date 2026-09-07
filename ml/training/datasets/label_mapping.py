"""
Mapeamento de rótulos de cada fonte para as três classes do projeto:
`safe`, `suspicious`, `phishing`.

Concentrar os dicionários aqui facilita ajustar o mapeamento quando os dados reais
forem inspecionados (ver "Riscos" no plano).

Contexto:
- **Sting9**: todas as categorias nativas são maliciosas — não há `safe`. As categorias
  reais da API (`GET /api/v1/stats` -> `submissions_by_category`) estão mapeadas abaixo:
  engano com intenção de roubo de credencial/pagamento -> `phishing`; golpe/spam de
  menor severidade -> `suspicious`; `other` é descartado.
  Rever este mapa após o 1º treino, olhando a matriz de confusão.
  Obs.: o dataset Sting9 é majoritariamente SMS (smishing), não e-mail corporativo.
- **Kaggle** (Phishing Email Dataset): binário — `safe` vs `phishing`. É a **única**
  fonte de exemplos `safe`.
"""
from __future__ import annotations

STING9_LABEL_MAPPING: dict[str, str] = {
    # -> phishing (engano direto, coleta de credenciais/pagamento)
    "phishing": "phishing",
    "smishing": "phishing",
    "sms phishing": "phishing",
    "account_alert": "phishing",
    "delivery": "phishing",
    "finance_crypto": "phishing",
    "prize_contest": "phishing",
    "spear phishing": "phishing",
    "whaling": "phishing",
    "bec": "phishing",
    "business email compromise": "phishing",
    # -> suspicious (spam / golpe de menor severidade)
    "spam": "suspicious",
    "advertisement": "suspicious",
    "job_advertisement": "suspicious",
    "loans_credit": "suspicious",
    "lawsuits_settlements": "suspicious",
    "tech_support_scam": "suspicious",
    "wrong_number_romance_scam": "suspicious",
    "romance scam": "suspicious",
    "scam": "suspicious",
    # "other" -> não mapeado (descartado)
}

KAGGLE_LABEL_MAPPING: dict[str, str] = {
    "1": "phishing",
    "phishing": "phishing",
    "phishing email": "phishing",
    "phishing_email": "phishing",
    "spam": "phishing",
    "fraud": "phishing",
    "malicious": "phishing",
    "0": "safe",
    "ham": "safe",
    "safe": "safe",
    "safe email": "safe",
    "safe_email": "safe",
    "legitimate": "safe",
    "legit": "safe",
    "normal": "safe",
    "not phishing": "safe",
}


class LabelMapper:
    """Mapeia um rótulo bruto (case-insensitive) para safe/suspicious/phishing."""

    def __init__(self, mapping: dict[str, str]):
        self._mapping = {self._key(k): v for k, v in mapping.items()}

    @staticmethod
    def _key(value) -> str:
        return str(value).strip().lower()

    def map(self, raw) -> str | None:
        """Retorna a classe canônica, ou None se o rótulo não for reconhecido."""
        return self._mapping.get(self._key(raw))


def sting9_mapper() -> LabelMapper:
    return LabelMapper(STING9_LABEL_MAPPING)


def kaggle_mapper() -> LabelMapper:
    return LabelMapper(KAGGLE_LABEL_MAPPING)
