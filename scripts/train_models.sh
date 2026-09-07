#!/usr/bin/env bash
# Consolida o dataset, treina e avalia os modelos (Naive Bayes + Random Forest).
#
# Variáveis de ambiente opcionais:
#   DATASET_PATH     - CSV consolidado de treino (default: ml/data/dataset.csv)
#   DATASET_SOURCES  - fontes para o build_dataset, separadas por vírgula
#                      (default: local_csv; ex.: "sting9,kaggle_phishing")
#   USE_SAMPLE_DATA  - "true" gera um dataset sintético e pula o build_dataset
#                      (apenas smoke test; train_model vai abortar por poucos dados)
#   KAGGLE_CSV       - caminho de um CSV do Kaggle já baixado (opcional)
#   STING9_EXPORT    - caminho de um export Sting9 já baixado (opcional)
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ML_DIR="$ROOT_DIR/ml"
DATASET_PATH="${DATASET_PATH:-$ML_DIR/data/dataset.csv}"
DATASET_SOURCES="${DATASET_SOURCES:-local_csv}"
USE_SAMPLE_DATA="${USE_SAMPLE_DATA:-false}"

cd "$ML_DIR"
VENV_ACTIVATE=""
for candidate in venv/bin/activate venv/Scripts/activate .venv/bin/activate .venv/Scripts/activate; do
    if [ -f "$candidate" ]; then
        VENV_ACTIVATE="$candidate"
        break
    fi
done
if [ -z "$VENV_ACTIVATE" ]; then
    echo "ERRO: ambiente virtual não encontrado em ml/venv nem ml/.venv." >&2
    echo "Crie com: python -m venv venv && venv/bin/pip install -r requirements.txt" >&2
    exit 1
fi
# shellcheck disable=SC1090
source "$VENV_ACTIVATE"

if [ "$USE_SAMPLE_DATA" = "true" ]; then
    echo "==> Gerando dataset sintético de exemplo (apenas smoke test)..."
    python -m training.generate_sample_dataset
else
    echo "==> Consolidando dataset a partir das fontes: $DATASET_SOURCES ..."
    build_args=(--sources "$DATASET_SOURCES" --out "$DATASET_PATH")
    [ -n "${KAGGLE_CSV:-}" ] && build_args+=(--kaggle-csv "$KAGGLE_CSV")
    [ -n "${STING9_EXPORT:-}" ] && build_args+=(--sting9-export "$STING9_EXPORT")
    python -m training.build_dataset "${build_args[@]}"
fi

echo "==> Treinando modelos (Naive Bayes + Random Forest) com validação cruzada k=10..."
python -m training.train_model --dataset "$DATASET_PATH"

echo "==> Avaliando modelos treinados..."
python -m training.evaluate_model --dataset "$DATASET_PATH"

deactivate
echo "Concluído. Modelos em ml/models/ | métricas em ml/models/metrics.json e METRICS.md"
