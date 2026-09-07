"""Testes do pacote de ingestão (`training.datasets`)."""
from pathlib import Path

import pandas as pd
import pytest

from training.datasets import (
    CANONICAL_COLUMNS,
    KagglePhishingSource,
    LocalCsvSource,
    Sting9Source,
    kaggle_mapper,
    normalize_frame,
    sting9_mapper,
)

FIXTURES = Path(__file__).resolve().parent / "fixtures"


class TestLabelMapper:
    def test_sting9_maps_attack_families(self):
        m = sting9_mapper()
        assert m.map("Phishing") == "phishing"
        assert m.map("BEC") == "phishing"
        assert m.map("smishing") == "phishing"
        assert m.map("Tech Support Scam") == "suspicious"
        assert m.map("Romance Scam") == "suspicious"
        assert m.map("Spam") == "suspicious"

    def test_sting9_is_case_insensitive_and_returns_none_for_unknown(self):
        m = sting9_mapper()
        assert m.map("  pHiShInG ") == "phishing"
        assert m.map("newsletter") is None

    def test_kaggle_binary_mapping(self):
        m = kaggle_mapper()
        assert m.map("Safe Email") == "safe"
        assert m.map("Phishing Email") == "phishing"
        assert m.map("0") == "safe"
        assert m.map("1") == "phishing"
        assert m.map("ham") == "safe"


class TestNormalizeFrame:
    def test_adds_missing_columns_and_fills_na(self):
        df = pd.DataFrame({"body": ["oi", None], "label": ["safe", "phishing"]})
        out = normalize_frame(df, source="t")
        assert list(out.columns) == CANONICAL_COLUMNS
        assert (out["subject"] == "").all()
        assert (out["sender"] == "").all()
        assert out.loc[1, "body"] == ""

    def test_applies_label_mapper_and_drops_unmapped(self, capsys):
        df = pd.DataFrame(
            {
                "body": ["a", "b", "c"],
                "label": ["Phishing", "Spam", "some junk label"],
            }
        )
        out = normalize_frame(df, source="sting9", label_mapper=sting9_mapper())
        assert list(out["label"]) == ["phishing", "suspicious"]
        assert "1 linha(s) descartada(s)" in capsys.readouterr().out

    def test_raises_when_nothing_valid_remains(self):
        df = pd.DataFrame({"body": ["a"], "label": ["nonsense"]})
        with pytest.raises(ValueError, match="nenhuma linha com rótulo válido"):
            normalize_frame(df, source="t", label_mapper=sting9_mapper())


class TestLocalCsvSource:
    def test_reads_canonical_csv(self):
        df = LocalCsvSource(FIXTURES / "canonical_sample.csv").load()
        assert list(df.columns) == CANONICAL_COLUMNS
        assert len(df) == 4
        assert set(df["label"]) == {"safe", "phishing", "suspicious"}

    def test_missing_file_raises(self, tmp_path):
        with pytest.raises(FileNotFoundError):
            LocalCsvSource(tmp_path / "nope.csv").load()


class TestKagglePhishingSource:
    def test_autodetects_columns_and_maps_binary_labels(self):
        df = KagglePhishingSource(FIXTURES / "kaggle_sample.csv", download=False).load()
        assert list(df.columns) == CANONICAL_COLUMNS
        assert df["label"].value_counts().to_dict() == {"safe": 3, "phishing": 3}
        # 'Email Text' vira body; sem colunas de subject/sender -> vazias
        assert (df["subject"] == "").all()
        assert df["body"].str.len().gt(0).all()

    def test_missing_csv_without_download_raises(self, tmp_path):
        src = KagglePhishingSource(download=False, dest_dir=tmp_path / "empty")
        with pytest.raises(FileNotFoundError):
            src.load()


class TestSting9Source:
    def test_flattens_export_and_maps_labels(self):
        df = Sting9Source(FIXTURES / "sting9_export_sample.json").load()
        assert list(df.columns) == CANONICAL_COLUMNS
        assert len(df) == 4  # 'Totally Unknown Label' descartado
        assert df["label"].value_counts().to_dict() == {"phishing": 2, "suspicious": 2}
        assert "Account security alert" in set(df["subject"])
        assert "ceo@company-x.co" in set(df["sender"])

    def test_missing_export_raises_with_hint(self, tmp_path):
        with pytest.raises(FileNotFoundError, match="fetch_sting9"):
            Sting9Source(tmp_path / "sting9_export_none.json").load()

    def test_unrecognized_structure_raises(self, tmp_path):
        bad = tmp_path / "bad.json"
        bad.write_text('{"nope": 1}', encoding="utf-8")
        with pytest.raises(ValueError, match="não reconhecida"):
            Sting9Source(bad).load()
