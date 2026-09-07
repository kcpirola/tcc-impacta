"""
Mapeamento de rótulos de cada fonte para as três classes do projeto:
`safe`, `suspicious`, `phishing`.

Concentrar os dicionários aqui facilita ajustar o mapeamento quando os dados reais
forem inspecionados (ver "Riscos" no plano).

Contexto:
- **Sting9**: todas as categorias nativas são maliciosas (Phishing, BEC, Smishing,
  Tech Support Scam, Romance Scam, Spam) — não há `safe`. Os ataques de engano direto
  viram `phishing`; golpes/spam de menor severidade viram `suspicious`.
- **Kaggle** (Phishing Email Dataset): binário — `safe` vs `phishing`. É a **única**
  fonte de exemplos `safe`.
"""
from __future__ import annotations

STING9_LABEL_MAPPING: dict[str, str] = {
    "phishing": "phishing",
    "spear phishing": "phishing",
    "whaling": "phishing",
    "bec": "phishing",
    "business email compromise": "phishing",
    "smishing": "phishing",
    "sms phishing": "phishing",
    "tech support scam": "suspicious",
    "tech_support_scam": "suspicious",
    "romance scam": "suspicious",
    "romance_scam": "suspicious",
    "spam": "suspicious",
    "scam": "suspicious",
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
