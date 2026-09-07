"""
Fonte de fallback: um CSV já no formato canônico (`subject,body,sender,label`).

Permite curar um dataset à mão e manter o pipeline funcionando sem acesso a
Sting9/Kaggle (usado como default no `build_dataset` e nos scripts de treino).
"""
from __future__ import annotations

from pathlib import Path

import pandas as pd

from config import Config
from training.datasets.base import normalize_frame


class LocalCsvSource:
    name = "local_csv"

    def __init__(self, path: str | Path | None = None):
        self._path = Path(path) if path else Config.PROCESSED_DATASET_PATH

    def load(self) -> pd.DataFrame:
        if not self._path.exists():
            raise FileNotFoundError(
                f"CSV local não encontrado: {self._path}. Informe --local-csv ou gere um "
                f"dataset de exemplo com `python -m training.generate_sample_dataset`."
            )
        df = pd.read_csv(self._path, dtype=str)
        # Já esperado no formato canônico; sem label_mapper (rótulos já são as 3 classes).
        return normalize_frame(df, source=self.name)
