"""Testes de `training.build_dataset` — consolidação, deduplicação e estatísticas."""
import json
from pathlib import Path

import pandas as pd
import pytest

from config import Config
from training.build_dataset import _deduplicate, build_dataset

FIXTURES = Path(__file__).resolve().parent / "fixtures"


class TestDeduplicate:
    def test_removes_exact_duplicates(self):
        row = {"subject": "Oi", "body": "corpo do email", "sender": "a@x.com", "label": "safe"}
        df = pd.DataFrame([row, row, row])
        assert len(_deduplicate(df)) == 1

    def test_removes_whitespace_only_variants(self):
        df = pd.DataFrame(
            [
                {"subject": "Fatura", "body": "confirme o pagamento", "sender": "v@x.com", "label": "suspicious"},
                {"subject": "Fatura", "body": "confirme   o   pagamento  ", "sender": "v@x.com", "label": "suspicious"},
                {"subject": "Fatura", "body": "outro corpo diferente", "sender": "v@x.com", "label": "suspicious"},
            ]
        )
        out = _deduplicate(df)
        assert len(out) == 2

    def test_keeps_distinct_rows(self):
        df = pd.DataFrame(
            [
                {"subject": "A", "body": "um", "sender": "a@x.com", "label": "safe"},
                {"subject": "B", "body": "dois", "sender": "b@x.com", "label": "phishing"},
            ]
        )
        assert len(_deduplicate(df)) == 2


class TestBuildDataset:
    @pytest.fixture
    def redirected_outputs(self, tmp_path, monkeypatch):
        out = tmp_path / "dataset.csv"
        stats = tmp_path / "dataset_stats.json"
        monkeypatch.setattr(Config, "PROCESSED_DATASET_PATH", out)
        monkeypatch.setattr(Config, "DATASET_STATS_PATH", stats)
        return out, stats

    def test_concatenates_sources_and_writes_outputs(self, redirected_outputs):
        out, stats_path = redirected_outputs

        result = build_dataset(
            ["local_csv", "kaggle_phishing"],
            local_csv=FIXTURES / "canonical_sample.csv",
            kaggle_csv=FIXTURES / "kaggle_sample.csv",
        )

        assert out.exists()
        df = pd.read_csv(out)
        assert list(df.columns) == ["subject", "body", "sender", "label"]
        # 4 (canonical) + 6 (kaggle) = 10, sem duplicatas entre as fontes
        assert len(df) == 10
        assert result["total_records"] == 10
        assert result["removed_duplicates"] == 0

        saved = json.loads(stats_path.read_text(encoding="utf-8"))
        assert saved["label_counts"] == result["label_counts"]
        assert set(saved["sources"]) == {"local_csv", "kaggle_phishing"}
        assert len(saved["sha256"]) == 64

    def test_deduplicates_across_sources(self, redirected_outputs, tmp_path):
        # dois CSVs canônicos com uma linha em comum
        a = tmp_path / "a.csv"
        b = tmp_path / "b.csv"
        a.write_text(
            "subject,body,sender,label\n"
            "Oi,corpo comum,x@x.com,safe\n"
            "Alerta,verifique sua conta,y@y.com,phishing\n",
            encoding="utf-8",
        )
        b.write_text(
            "subject,body,sender,label\n"
            "Oi,corpo   comum,x@x.com,safe\n"
            "Nova,mensagem distinta,z@z.com,suspicious\n",
            encoding="utf-8",
        )

        # duas instâncias da fonte local_csv não são possíveis via nome repetido;
        # usa build direto concatenando via arquivo intermediário
        from training.datasets import LocalCsvSource
        from training.build_dataset import _deduplicate

        combined = pd.concat(
            [LocalCsvSource(a).load(), LocalCsvSource(b).load()], ignore_index=True
        )
        deduped = _deduplicate(combined)
        assert len(combined) == 4
        assert len(deduped) == 3

    def test_unknown_source_raises(self, redirected_outputs):
        with pytest.raises(ValueError, match="desconhecida"):
            build_dataset(["nao_existe"], local_csv=FIXTURES / "canonical_sample.csv")
