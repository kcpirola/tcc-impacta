#!/usr/bin/env bash
# Treina e avalia os modelos de classificação (Naive Bayes + Random Forest).
# Variáveis de ambiente opcionais:
#   DATASET_PATH   - caminho do CSV de treino (default: ml/data/dataset.csv)
#   USE_SAMPLE_DATA - "true" para gerar um dataset sintético antes de treinar
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ML_DIR="$ROOT_DIR/ml"
DATASET_PATH="${DATASET_PATH:-$ML_DIR/data/dataset.csv}"
USE_SAMPLE_DATA="${USE_SAMPLE_DATA:-false}"

cd "$ML_DIR"
source .venv/bin/activate

if [ "$USE_SAMPLE_DATA" = "true" ] || [ ! -f "$DATASET_PATH" ]; then
    echo "==> Gerando dataset sintético de exemplo (apenas para smoke test)..."
    python -m training.generate_sample_dataset
fi

echo "==> Treinando modelos (Naive Bayes + Random Forest) com validação cruzada k=10..."
python -m training.train_model --dataset "$DATASET_PATH"

echo "==> Avaliando modelos treinados..."
python -m training.evaluate_model --dataset "$DATASET_PATH"

deactivate
echo "Treinamento concluído. Modelos salvos em ml/models/"
