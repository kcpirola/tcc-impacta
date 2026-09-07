"""
Orquestrador de datasets: carrega as fontes escolhidas, concatena, remove duplicatas
e grava o CSV canônico consumido por `train_model` / `evaluate_model`.

Uso:
    python -m training.build_dataset --sources sting9,kaggle_phishing
    python -m training.build_dataset --sources local_csv --local-csv data/dataset.csv
"""
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

import pandas as pd

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config
from training.data_preprocessing import EmailPreprocessor
from training.datasets import KagglePhishingSource, LocalCsvSource, Sting9Source

# Nome da fonte -> fábrica que recebe os kwargs do build e devolve a instância.
SOURCE_FACTORIES = {
    "local_csv": lambda o: LocalCsvSource(o.get("local_csv")),
    "kaggle_phishing": lambda o: KagglePhishingSource(
        o.get("kaggle_csv"), download=o.get("download", False), slug=o.get("kaggle_slug")
    ),
    # CSV binário de spam tratado como classe intermediária (rótulo 1 -> suspicious).
    "spam_csv": lambda o: KagglePhishingSource(
        o.get("spam_csv"), positive_label="suspicious"
    ),
    "sting9": lambda o: Sting9Source(o.get("sting9_export")),
}


def _deduplicate(df: pd.DataFrame) -> pd.DataFrame:
    """Remove duplicatas exatas e variações que diferem apenas por espaços no corpo."""
    key = pd.DataFrame(
        {
            "subject": df["subject"].str.strip().str.lower(),
            "sender": df["sender"].str.strip().str.lower(),
            "body": df["body"].map(EmailPreprocessor.remove_extra_whitespace).str.lower(),
        }
    )
    keep = ~key.duplicated(keep="first")
    return df[keep.values].reset_index(drop=True)


def build_dataset(
    sources: list[str],
    out_path: str | Path | None = None,
    *,
    local_csv: str | Path | None = None,
    kaggle_csv: str | Path | None = None,
    spam_csv: str | Path | None = None,
    kaggle_slug: str | None = None,
    sting9_export: str | Path | None = None,
    download: bool = False,
) -> dict:
    """Executa a consolidação e devolve o dicionário de estatísticas gravado em disco."""
    if not sources:
        raise ValueError("Informe ao menos uma fonte em --sources.")

    unknown = [s for s in sources if s not in SOURCE_FACTORIES]
    if unknown:
        raise ValueError(
            f"Fonte(s) desconhecida(s): {unknown}. Disponíveis: {sorted(SOURCE_FACTORIES)}"
        )

    out_path = Path(out_path) if out_path else Config.PROCESSED_DATASET_PATH
    opts = {
        "local_csv": local_csv,
        "kaggle_csv": kaggle_csv,
        "spam_csv": spam_csv,
        "kaggle_slug": kaggle_slug,
        "sting9_export": sting9_export,
        "download": download,
    }

    frames: list[pd.DataFrame] = []
    per_source: dict[str, dict] = {}
    for name in sources:
        source = SOURCE_FACTORIES[name](opts)
        df = source.load()
        counts = df["label"].value_counts().to_dict()
        per_source[name] = {"records": int(len(df)), "label_counts": counts}
        print(f"[{name}] {len(df)} linha(s) -> {counts}")
        frames.append(df)

    combined = pd.concat(frames, ignore_index=True)
    before = len(combined)
    combined = _deduplicate(combined)
    removed = before - len(combined)
    print(f"Duplicatas removidas: {removed}")

    total_counts = combined["label"].value_counts().to_dict()
    print(f"Total consolidado: {len(combined)} linha(s) -> {total_counts}")

    out_path.parent.mkdir(parents=True, exist_ok=True)
    combined.to_csv(out_path, index=False)

    stats = {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "output": str(out_path),
        "total_records": int(len(combined)),
        "removed_duplicates": int(removed),
        "label_counts": total_counts,
        "sources": per_source,
        "sha256": hashlib.sha256(out_path.read_bytes()).hexdigest(),
    }
    # Estatísticas ficam ao lado do CSV gerado (data/dataset_stats.json no caso padrão).
    stats_path = out_path.with_name("dataset_stats.json")
    stats_path.write_text(json.dumps(stats, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Dataset: {out_path}")
    print(f"Estatísticas: {stats_path}")
    return stats


def main() -> None:
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except (AttributeError, ValueError):
        pass

    parser = argparse.ArgumentParser(description="Consolida datasets de e-mails para treino")
    parser.add_argument(
        "--sources",
        required=True,
        help="Fontes separadas por vírgula: sting9, kaggle_phishing, local_csv",
    )
    parser.add_argument("--out", default=None, help="CSV de saída (default: data/dataset.csv)")
    parser.add_argument("--local-csv", default=None, help="Caminho do CSV canônico (fonte local_csv)")
    parser.add_argument("--kaggle-csv", default=None, help="CSV binário do Kaggle (rótulo 1 -> phishing)")
    parser.add_argument("--spam-csv", default=None, help="CSV binário de spam (rótulo 1 -> suspicious)")
    parser.add_argument("--kaggle-slug", default=None, help="owner/dataset do Kaggle p/ download")
    parser.add_argument("--sting9-export", default=None, help="JSON do export Sting9 já baixado")
    parser.add_argument(
        "--download", action="store_true", help="Permite baixar o dataset do Kaggle via CLI"
    )
    args = parser.parse_args()

    sources = [s.strip() for s in args.sources.split(",") if s.strip()]
    try:
        build_dataset(
            sources,
            args.out,
            local_csv=args.local_csv,
            kaggle_csv=args.kaggle_csv,
            spam_csv=args.spam_csv,
            kaggle_slug=args.kaggle_slug,
            sting9_export=args.sting9_export,
            download=args.download,
        )
    except (ValueError, FileNotFoundError) as exc:
        print(f"\nErro: {exc}", file=sys.stderr)
        raise SystemExit(1)


if __name__ == "__main__":
    main()
