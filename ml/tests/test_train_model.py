"""
Testes para training.train_model.

Cobrem a guarda de k-fold (`_resolve_cv_folds`) — aborta quando o dataset é pequeno
demais e reduz k quando necessário — e a persistência de `cv_metrics.json` pelo `train`.
"""
import json

import joblib
import pandas as pd
import pytest

from config import Config
from training.train_model import _resolve_cv_folds, train


class TestResolveCvFolds:
    def test_aborts_when_a_class_is_missing(self, monkeypatch):
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 1)
        counts = pd.Series({"safe": 100, "phishing": 100})  # falta 'suspicious'
        with pytest.raises(ValueError, match="não contém as classes"):
            _resolve_cv_folds(counts)

    def test_aborts_when_class_below_minimum(self, monkeypatch):
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 50)
        counts = pd.Series({"safe": 200, "suspicious": 200, "phishing": 12})
        with pytest.raises(ValueError, match="menos populada"):
            _resolve_cv_folds(counts)

    def test_reduces_k_when_between_minimum_and_cv_folds(self, monkeypatch, capsys):
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 3)
        monkeypatch.setattr(Config, "CV_FOLDS", 10)
        counts = pd.Series({"safe": 40, "suspicious": 40, "phishing": 6})
        k = _resolve_cv_folds(counts)
        assert k == 6
        assert "reduzindo k" in capsys.readouterr().out

    def test_keeps_configured_k_when_enough_samples(self, monkeypatch):
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 3)
        monkeypatch.setattr(Config, "CV_FOLDS", 10)
        counts = pd.Series({"safe": 50, "suspicious": 50, "phishing": 50})
        assert _resolve_cv_folds(counts) == 10


class TestTrain:
    @pytest.fixture
    def dataset_csv(self, tmp_path, monkeypatch):
        monkeypatch.setattr(Config, "MODELS_DIR", tmp_path)
        monkeypatch.setattr(Config, "NAIVE_BAYES_MODEL_PATH", tmp_path / "naive_bayes_model.pkl")
        monkeypatch.setattr(Config, "RANDOM_FOREST_MODEL_PATH", tmp_path / "random_forest_model.pkl")
        monkeypatch.setattr(Config, "TFIDF_VECTORIZER_PATH", tmp_path / "tfidf_vectorizer.pkl")
        monkeypatch.setattr(Config, "FEATURE_SCALER_PATH", tmp_path / "feature_scaler.pkl")
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 4)
        monkeypatch.setattr(Config, "CV_FOLDS", 3)
        monkeypatch.setattr(Config, "TFIDF_MAX_FEATURES", 100)

        safe = [
            ("Reuniao de equipe", "Confirmando nossa reuniao de equipe as dez horas na sala tres."),
            ("Relatorio mensal de vendas", "Segue anexo o relatorio mensal de vendas fechado ontem."),
            ("Agenda da semana", "Segue a agenda com as reunioes e compromissos desta semana."),
            ("Ferias aprovadas", "Suas ferias foram aprovadas pelo gestor para o proximo mes."),
            ("Documentacao atualizada", "A documentacao tecnica do projeto foi atualizada no repositorio."),
        ]
        suspicious = [
            ("Confirme seus dados cadastrais", "Prezado cliente confirme seus dados de acesso no formulario indicado."),
            ("Atualizacao de seguranca pendente", "Detectamos atividade incomum recomendamos revisar sua conta em breve."),
            ("Fatura pendente em anexo", "Segue fatura em anexo favor confirmar o pagamento ate amanha."),
            ("Revisao de acesso necessaria", "Sua conta precisa de revisao de acesso responda com seus dados."),
            ("Comprovante solicitado", "Encaminhe o comprovante de pagamento respondendo este email o quanto antes."),
        ]
        phishing = [
            ("URGENTE sua conta sera suspensa", "Clique aqui imediatamente para verificar sua conta http://192.168.10.5/login"),
            ("Verifique sua senha agora", "Sua senha expira hoje clique aqui http://bit.ly/verify caro cliente aja agora"),
            ("Premio exclusivo aja agora", "Parabens voce ganhou um premio clique aqui http://tinyurl.com/premio para resgatar"),
            ("Confirmacao de conta necessaria", "Caro cliente detectamos atividade incomum confirme sua conta http://200.15.3.8/confirm"),
            ("Acesso bloqueado reative agora", "Sua conta foi bloqueada reative imediatamente em http://163.10.2.9/unlock urgente"),
        ]

        rows = []
        for subject, body in safe:
            rows.append((subject, body, "colega@empresa.com", "safe"))
        for subject, body in suspicious:
            rows.append((subject, body, "suporte@parceiro-externo.com", "suspicious"))
        for subject, body in phishing:
            rows.append((subject, body, "alert@bank-verify.tk", "phishing"))

        csv_path = tmp_path / "dataset.csv"
        pd.DataFrame(rows, columns=["subject", "body", "sender", "label"]).to_csv(csv_path, index=False)
        return csv_path, tmp_path

    def test_train_writes_models_and_cv_metrics(self, dataset_csv):
        csv_path, models_dir = dataset_csv

        train(csv_path)

        for name in (
            "naive_bayes_model.pkl", "random_forest_model.pkl", "tfidf_vectorizer.pkl",
            "feature_scaler.pkl", "label_encoder.pkl",
        ):
            assert (models_dir / name).exists(), f"faltou {name}"

        cv = json.loads((models_dir / "cv_metrics.json").read_text(encoding="utf-8"))
        assert cv["cv_folds"] == 3
        assert cv["scoring"] == "accuracy"
        for model_key in ("naive_bayes", "random_forest"):
            assert len(cv[model_key]["scores"]) == 3
            assert 0.0 <= cv[model_key]["mean"] <= 1.0

    def test_random_forest_uses_sklearn_defaults(self, dataset_csv):
        csv_path, models_dir = dataset_csv

        train(csv_path)

        rf = joblib.load(models_dir / "random_forest_model.pkl")
        # Parâmetros padrão do scikit-learn (metodologia 2.4.3); só RANDOM_STATE é fixado.
        assert rf.n_estimators == 100
        assert rf.min_samples_split == 2
        assert rf.class_weight is None
        assert rf.max_depth is None
        assert rf.random_state == Config.RANDOM_STATE

    def test_train_aborts_on_synthetic_sample_dataset(self, tmp_path, monkeypatch):
        """O dataset sintético (poucas amostras por classe) não deve treinar."""
        monkeypatch.setattr(Config, "MODELS_DIR", tmp_path)
        monkeypatch.setattr(Config, "MIN_SAMPLES_PER_CLASS", 50)
        rows = (
            [("s", "corpo seguro numero um", "a@x.com", "safe")] * 8
            + [("s", "corpo suspeito numero um", "b@x.com", "suspicious")] * 3
            + [("s", "corpo phishing numero um", "c@x.com", "phishing")] * 4
        )
        csv_path = tmp_path / "dataset.csv"
        pd.DataFrame(rows, columns=["subject", "body", "sender", "label"]).to_csv(csv_path, index=False)

        with pytest.raises(ValueError, match="menos populada"):
            train(csv_path)
