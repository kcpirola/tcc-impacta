"""
Coleta do dataset Sting9 via API autenticada (passo manual, fora do fluxo de treino).

Fluxo:
    1. Registrar em sting9.org  -> POST /api/v1/auth/register
    2. Login                    -> POST /api/v1/auth/login  (copiar o JWT p/ STING9_TOKEN)
    3. python -m training.fetch_sting9 --out data/raw/sting9_export_<data>.json

Tenta primeiro `GET {STING9_API_BASE}/export?format=json`; se indisponível, pagina
`GET {STING9_API_BASE}/submissions`. Grava o JSON bruto + um `<arquivo>.meta.json`.
Depende apenas de `requests`.
"""
from __future__ import annotations

import argparse
import json
import sys
import time
from datetime import datetime, timezone
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config

PAGE_SIZE = 100
_RATE_SLEEP_SECONDS = 0.2  # ~1000 req/h de folga


class _ExportUnavailable(Exception):
    """O endpoint /export não existe nesta instância; usar paginação."""


def _make_session(token: str):
    import requests  # import tardio: só necessário para a coleta real

    session = requests.Session()
    session.headers.update({"Authorization": f"Bearer {token}", "Accept": "application/json"})
    return session


def _as_records(payload) -> list:
    if isinstance(payload, list):
        return payload
    if isinstance(payload, dict):
        for key in ("submissions", "data", "results", "items", "records"):
            if isinstance(payload.get(key), list):
                return payload[key]
    return []


def _fetch_export(session, api_base: str) -> list:
    response = session.get(f"{api_base}/export", params={"format": "json"})
    if response.status_code in (404, 405, 501):
        raise _ExportUnavailable()
    response.raise_for_status()
    records = _as_records(response.json())
    if not records:
        raise _ExportUnavailable()
    return records


def _fetch_paginated(session, api_base: str, max_pages: int | None) -> list:
    records: list = []
    page = 1
    while max_pages is None or page <= max_pages:
        response = session.get(
            f"{api_base}/submissions", params={"page": page, "page_size": PAGE_SIZE}
        )
        response.raise_for_status()
        batch = _as_records(response.json())
        if not batch:
            break
        records.extend(batch)
        page += 1
        time.sleep(_RATE_SLEEP_SECONDS)
    return records


def fetch(
    out_path: str | Path,
    *,
    api_base: str | None = None,
    token: str | None = None,
    session=None,
    max_pages: int | None = None,
) -> dict:
    """Baixa o dataset e grava `out_path` + `out_path.meta.json`. Retorna o meta."""
    api_base = (api_base or Config.STING9_API_BASE).rstrip("/")
    token = token if token is not None else Config.STING9_TOKEN
    if not token:
        raise ValueError(
            "STING9_TOKEN (ou STING9_API_KEY) não configurado. Registre-se em sting9.org, "
            "faça login e exporte o JWT em STING9_TOKEN antes de rodar este script."
        )

    session = session or _make_session(token)

    endpoint = "export"
    try:
        records = _fetch_export(session, api_base)
    except _ExportUnavailable:
        print("Endpoint /export indisponível; paginando /submissions ...")
        endpoint = "submissions"
        records = _fetch_paginated(session, api_base, max_pages)

    out_path = Path(out_path)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(
        json.dumps({"submissions": records}, ensure_ascii=False, indent=2), encoding="utf-8"
    )

    meta = {
        "fetched_at": datetime.now(timezone.utc).isoformat(),
        "api_base": api_base,
        "endpoint": endpoint,
        "record_count": len(records),
    }
    out_path.with_suffix(out_path.suffix + ".meta.json").write_text(
        json.dumps(meta, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    print(f"{len(records)} registro(s) via /{endpoint} -> {out_path}")
    return meta


def main() -> None:
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

    default_out = Config.RAW_DATA_DIR / f"sting9_export_{datetime.now():%Y-%m-%d}.json"
    parser = argparse.ArgumentParser(description="Baixa o dataset Sting9 via API")
    parser.add_argument("--out", default=str(default_out), help="Caminho do JSON de saída")
    parser.add_argument("--api-base", default=None, help="Override de STING9_API_BASE")
    parser.add_argument("--max-pages", type=int, default=None, help="Limite de páginas (fallback)")
    args = parser.parse_args()

    try:
        fetch(args.out, api_base=args.api_base, max_pages=args.max_pages)
    except ValueError as exc:
        print(f"\nErro: {exc}", file=sys.stderr)
        raise SystemExit(1)


if __name__ == "__main__":
    main()
