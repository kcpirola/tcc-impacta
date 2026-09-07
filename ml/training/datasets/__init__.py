"""
Pacote de ingestão de dados: fontes plugáveis que devolvem e-mails rotulados no
esquema canônico `subject,body,sender,label` (`label ∈ {safe, suspicious, phishing}`).
"""
from training.datasets.base import (
    CANONICAL_COLUMNS,
    VALID_LABELS,
    DatasetSource,
    load_all_sources,
    normalize_frame,
)
from training.datasets.kaggle_phishing import KagglePhishingSource
from training.datasets.label_mapping import LabelMapper, kaggle_mapper, sting9_mapper
from training.datasets.local_csv import LocalCsvSource
from training.datasets.sting9 import Sting9Source

__all__ = [
    "CANONICAL_COLUMNS",
    "VALID_LABELS",
    "DatasetSource",
    "normalize_frame",
    "load_all_sources",
    "LabelMapper",
    "sting9_mapper",
    "kaggle_mapper",
    "LocalCsvSource",
    "KagglePhishingSource",
    "Sting9Source",
]
