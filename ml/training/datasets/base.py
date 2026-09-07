"""
Contrato comum das fontes de dados e normalização para o esquema canônico.

Toda fonte (`DatasetSource`) devolve um `DataFrame` com as colunas
`["subject", "body", "sender", "label"]`, `label ∈ {safe, suspicious, phishing}`.
"""
from __future__ import annotations

from typing import Iterable, Protocol, runtime_checkable

import pandas as pd

CANONICAL_COLUMNS: list[str] = ["subject", "body", "sender", "label"]
VALID_LABELS: set[str] = {"safe", "suspicious", "phishing"}


@runtime_checkable
class DatasetSource(Protocol):
    """Fonte de e-mails rotulados. Implementações vivem em `training/datasets/`."""

    name: str

    def load(self) -> pd.DataFrame:
        """Retorna um DataFrame já no esquema canônico (ver `normalize_frame`)."""
        ...


def normalize_frame(
    df: pd.DataFrame,
    *,
    source: str,
    label_mapper=None,
    column_aliases: dict[str, Iterable[str]] | None = None,
) -> pd.DataFrame:
    """
    Converte um DataFrame arbitrário para o esquema canônico.

    - `column_aliases`: {coluna_canônica: [nomes possíveis na origem]}. A primeira que
      existir é renomeada; colunas canônicas ausentes viram string vazia.
    - `label_mapper`: opcional; mapeia o rótulo bruto para safe/suspicious/phishing.
      Linhas cujo rótulo não mapeia (ou não está em VALID_LABELS) são descartadas.
    - Todos os campos de texto viram `str` sem NaN.

    Levanta ValueError se, após a normalização, não sobrar nenhuma linha válida.
    """
    df = df.copy()

    if column_aliases:
        for canonical, candidates in column_aliases.items():
            if canonical in df.columns:
                continue
            for cand in candidates:
                if cand in df.columns:
                    df = df.rename(columns={cand: canonical})
                    break

    for col in CANONICAL_COLUMNS:
        if col not in df.columns:
            df[col] = ""

    df = df[CANONICAL_COLUMNS]

    for col in ("subject", "body", "sender"):
        df[col] = df[col].fillna("").astype(str)

    raw_label = df["label"].fillna("").astype(str).str.strip()
    if label_mapper is not None:
        mapped = raw_label.map(label_mapper.map)
    else:
        mapped = raw_label.str.lower()

    df["label"] = mapped
    valid = df["label"].isin(VALID_LABELS)
    dropped = int((~valid).sum())
    if dropped:
        print(f"[{source}] {dropped} linha(s) descartada(s): rótulo não reconhecido")

    df = df[valid].reset_index(drop=True)
    if df.empty:
        raise ValueError(
            f"[{source}] nenhuma linha com rótulo válido após a normalização "
            f"(esperado um de {sorted(VALID_LABELS)})."
        )
    return df


def load_all_sources(sources: Iterable[DatasetSource]) -> pd.DataFrame:
    """Carrega e concatena várias fontes já normalizadas, na ordem informada."""
    frames = [src.load() for src in sources]
    if not frames:
        raise ValueError("Nenhuma fonte de dados informada.")
    return pd.concat(frames, ignore_index=True)
