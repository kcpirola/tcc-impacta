"""
Fonte Kaggle — "Phishing Email Dataset".

Aceita um CSV já baixado (`csv_path`) ou, se `download=True` e houver credenciais
Kaggle no ambiente (`KAGGLE_USERNAME`/`KAGGLE_KEY`), baixa via CLI `kaggle`.

O dataset é binário (safe vs phishing) e tem nomes de coluna variáveis entre versões
(`Email Text`/`text`/`body`, `Email Type`/`label`/`0-1`); ambos são autodetectados.
"""
from __future__ import annotations

import glob
import os
import subprocess
import sys
from pathlib import Path

import pandas as pd

from config import Config
from training.datasets.base import normalize_frame
from training.datasets.label_mapping import kaggle_mapper

_BODY_ALIASES = ["body", "email text", "email_text", "text", "message", "content", "email", "e-mail text"]
_SUBJECT_ALIASES = ["subject", "email subject", "title"]
_SENDER_ALIASES = ["sender", "from", "email_from", "sender_email"]
_LABEL_ALIASES = ["label", "email type", "email_type", "type", "class", "category", "target", "is_phishing"]


def _find_column(columns: list[str], aliases: list[str]) -> str | None:
    lookup = {c.strip().lower(): c for c in columns}
    for alias in aliases:
        if alias in lookup:
            return lookup[alias]
    return None


class KagglePhishingSource:
    name = "kaggle_phishing"

    def __init__(
        self,
        csv_path: str | Path | None = None,
        *,
        download: bool = False,
        slug: str | None = None,
        dest_dir: str | Path | None = None,
    ):
        self._csv_path = Path(csv_path) if csv_path else None
        self._download = download
        self._slug = slug or Config.KAGGLE_DATASET_SLUG
        self._dest_dir = Path(dest_dir) if dest_dir else (Config.RAW_DATA_DIR / "kaggle")

    def load(self) -> pd.DataFrame:
        csv_path = self._resolve_csv()
        df = pd.read_csv(csv_path, dtype=str)

        cols = list(df.columns)
        column_aliases = {
            "body": [_find_column(cols, _BODY_ALIASES) or "body"],
            "subject": [_find_column(cols, _SUBJECT_ALIASES) or "subject"],
            "sender": [_find_column(cols, _SENDER_ALIASES) or "sender"],
            "label": [_find_column(cols, _LABEL_ALIASES) or "label"],
        }
        if _find_column(cols, _BODY_ALIASES) is None and _find_column(cols, _LABEL_ALIASES) is None:
            raise ValueError(
                f"Não consegui identificar as colunas de texto/rótulo em {csv_path}. "
                f"Colunas encontradas: {cols}"
            )
        return normalize_frame(
            df, source=self.name, label_mapper=kaggle_mapper(), column_aliases=column_aliases
        )

    def _resolve_csv(self) -> Path:
        if self._csv_path:
            if not self._csv_path.exists():
                raise FileNotFoundError(f"CSV do Kaggle não encontrado: {self._csv_path}")
            return self._csv_path

        found = self._first_csv_in(self._dest_dir)
        if found:
            return found

        if self._download:
            self._kaggle_download()
            found = self._first_csv_in(self._dest_dir)
            if found:
                return found

        raise FileNotFoundError(
            "Nenhum CSV do Kaggle disponível. Baixe o 'Phishing Email Dataset' e passe "
            f"--kaggle-csv <arquivo.csv>, ou configure KAGGLE_USERNAME/KAGGLE_KEY e "
            f"KAGGLE_DATASET_SLUG para baixar automaticamente. Procurei em: {self._dest_dir}"
        )

    @staticmethod
    def _first_csv_in(directory: Path) -> Path | None:
        if not directory.exists():
            return None
        matches = sorted(glob.glob(str(directory / "**" / "*.csv"), recursive=True))
        return Path(matches[0]) if matches else None

    def _kaggle_download(self) -> None:
        if not self._slug:
            raise ValueError("KAGGLE_DATASET_SLUG não configurado (formato: owner/dataset).")
        if not (os.getenv("KAGGLE_USERNAME") and os.getenv("KAGGLE_KEY")):
            raise ValueError("KAGGLE_USERNAME/KAGGLE_KEY ausentes no ambiente.")
        self._dest_dir.mkdir(parents=True, exist_ok=True)
        cmd = [
            sys.executable, "-m", "kaggle", "datasets", "download",
            "-d", self._slug, "-p", str(self._dest_dir), "--unzip",
        ]
        print(f"[kaggle_phishing] baixando {self._slug} ...")
        subprocess.run(cmd, check=True)
