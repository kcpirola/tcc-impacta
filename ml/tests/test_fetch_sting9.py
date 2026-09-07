"""Testes de `training.fetch_sting9` com uma sessão HTTP falsa (sem rede)."""
import json

import pytest

from config import Config
from training.fetch_sting9 import fetch

RECORD = {"category": "Phishing", "content": "x", "metadata": {"subject": "s", "sender": "a@x.com"}}


class FakeResponse:
    def __init__(self, status_code, payload):
        self.status_code = status_code
        self._payload = payload

    def json(self):
        return self._payload

    def raise_for_status(self):
        if self.status_code >= 400:
            raise RuntimeError(f"HTTP {self.status_code}")


class FakeSession:
    def __init__(self, handler):
        self._handler = handler
        self.calls = []

    def get(self, url, params=None):
        params = params or {}
        self.calls.append((url, params))
        return self._handler(url, params)


@pytest.fixture(autouse=True)
def _clear_token(monkeypatch):
    monkeypatch.setattr(Config, "STING9_TOKEN", "")


def test_uses_export_endpoint_when_available(tmp_path):
    def handler(url, params):
        assert url.endswith("/export")
        return FakeResponse(200, {"submissions": [RECORD, RECORD, RECORD]})

    out = tmp_path / "sting9_export.json"
    meta = fetch(out, api_base="https://sting9.test/api/v1", token="jwt", session=FakeSession(handler))

    assert meta["endpoint"] == "export"
    assert meta["record_count"] == 3
    saved = json.loads(out.read_text(encoding="utf-8"))
    assert len(saved["submissions"]) == 3
    meta_file = json.loads((tmp_path / "sting9_export.json.meta.json").read_text(encoding="utf-8"))
    assert meta_file["endpoint"] == "export"


def test_falls_back_to_pagination_when_export_missing(tmp_path):
    def handler(url, params):
        if url.endswith("/export"):
            return FakeResponse(404, {})
        assert url.endswith("/submissions")
        if params.get("page") == 1:
            return FakeResponse(200, {"submissions": [RECORD, RECORD]})
        return FakeResponse(200, {"submissions": []})

    out = tmp_path / "sting9_export.json"
    meta = fetch(out, api_base="https://sting9.test/api/v1", token="jwt", session=FakeSession(handler))

    assert meta["endpoint"] == "submissions"
    assert meta["record_count"] == 2
    saved = json.loads(out.read_text(encoding="utf-8"))
    assert len(saved["submissions"]) == 2


def test_raises_without_token(tmp_path):
    with pytest.raises(ValueError, match="STING9_TOKEN"):
        fetch(tmp_path / "x.json", token="", session=FakeSession(lambda u, p: FakeResponse(200, {})))
