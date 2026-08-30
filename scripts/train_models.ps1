# Treina e avalia os modelos de classificação (Naive Bayes + Random Forest).
# Variáveis de ambiente opcionais:
#   DATASET_PATH    - caminho do CSV de treino (default: ml/data/dataset.csv)
#   USE_SAMPLE_DATA - "true" para gerar um dataset sintético antes de treinar
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$MlDir = Join-Path $RootDir "ml"
$DatasetPath = if ($env:DATASET_PATH) { $env:DATASET_PATH } else { Join-Path $MlDir "data\dataset.csv" }
$UseSampleData = if ($env:USE_SAMPLE_DATA) { $env:USE_SAMPLE_DATA } else { "false" }

Push-Location $MlDir
$python = ".venv\Scripts\python.exe"

if ($UseSampleData -eq "true" -or -not (Test-Path $DatasetPath)) {
    Write-Host "==> Gerando dataset sintético de exemplo (apenas para smoke test)..." -ForegroundColor Cyan
    & $python -m training.generate_sample_dataset
}

Write-Host "==> Treinando modelos (Naive Bayes + Random Forest) com validação cruzada k=10..." -ForegroundColor Cyan
& $python -m training.train_model --dataset $DatasetPath

Write-Host "==> Avaliando modelos treinados..." -ForegroundColor Cyan
& $python -m training.evaluate_model --dataset $DatasetPath

Pop-Location
Write-Host "Treinamento concluído. Modelos salvos em ml\models\" -ForegroundColor Green
