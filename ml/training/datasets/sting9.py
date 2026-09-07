"""
Fonte primária — Sting9 Research Initiative.

Lê um export JSON **já baixado** (produzido por `training.fetch_sting9`), sem nenhuma
chamada de rede no caminho de treino, para manter os experimentos reprodutíveis.

O formato exato do export não é documentado publicamente; o achatamento de cada
submissão está isolado em `_flatten_record` e tolera chaves ausentes.
"""
from __future__ import annotations

import json
from pathlib import Path

import pandas as pd

from config import Config
from training.datasets.base import normalize_frame
from training.datasets.label_mapping import sting9_mapper

# Chaves toleradas (case-insensitive). O export atual da API entrega
# `anonymized_content` (texto único, sem subject/sender) + `category` + `type`.
_RAW_LABEL_KEYS = ("category", "attack_type", "type", "threat_type", "classification", "label")
_SUBJECT_KEYS = ("subject", "title", "headline")
_BODY_KEYS = ("anonymized_content", "anonymizedcontent", "content", "body", "text", "message", "raw_text")
_SENDER_KEYS = ("sender", "from", "sender_address", "sender_domain", "from_address")


def _lower_keys(mapping: dict) -> dict:
    return {str(k).lower(): v for k, v in mapping.items()}


def _first(mapping: dict, keys: tuple[str, ...]) -> str:
    for key in keys:
        value = mapping.get(key)
        if value:
            return str(value)
    return ""


def _flatten_record(record: dict) -> dict:
    rec = _lower_keys(record)
    meta = rec.get("metadata") or rec.get("meta") or rec.get("email_details") or {}
    meta = _lower_keys(meta) if isinstance(meta, dict) else {}
    return {
        "subject": _first(rec, _SUBJECT_KEYS) or _first(meta, _SUBJECT_KEYS),
        "body": _first(rec, _BODY_KEYS) or _first(meta, _BODY_KEYS),
        "sender": _first(rec, _SENDER_KEYS) or _first(meta, _SENDER_KEYS),
        "label": _first(rec, _RAW_LABEL_KEYS) or _first(meta, _RAW_LABEL_KEYS),
    }


def _extract_records(payload) -> list[dict]:
    if isinstance(payload, list):
        return [r for r in payload if isinstance(r, dict)]
    if isinstance(payload, dict):
        for key in ("submissions", "data", "results", "items", "records"):
            value = payload.get(key)
            if isinstance(value, list):
                return [r for r in value if isinstance(r, dict)]
    raise ValueError(
        "Estrutura do export Sting9 não reconhecida (esperado uma lista ou "
        "{'submissions': [...]}). Verifique o arquivo gerado por fetch_sting9."
    )


class Sting9Source:
    name = "sting9"

    def __init__(self, export_path: str | Path | None = None):
        self._export_path = Path(export_path) if export_path else None

    def load(self) -> pd.DataFrame:
        path = self._resolve_export_path()
        if path is None or not path.exists():
            raise FileNotFoundError(
                "Nenhum export do Sting9 encontrado. Rode "
                "`python -m training.fetch_sting9 --out data/raw/sting9_export_<data>.json` "
                "(requer STING9_TOKEN) ou passe --sting9-export <arquivo.json>."
            )
        payload = json.loads(path.read_text(encoding="utf-8"))
        records = _extract_records(payload)
        rows = [_flatten_record(r) for r in records]
        df = pd.DataFrame(rows, columns=["subject", "body", "sender", "label"])
        return normalize_frame(df, source=self.name, label_mapper=sting9_mapper())

    def _resolve_export_path(self) -> Path | None:
        if self._export_path:
            return self._export_path
        if Config.STING9_EXPORT_PATH:
            return Path(Config.STING9_EXPORT_PATH)
        candidates = sorted(Config.RAW_DATA_DIR.glob("sting9_export_*.json"))
        return candidates[-1] if candidates else None
