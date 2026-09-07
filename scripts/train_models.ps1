# Consolida o dataset, treina e avalia os modelos (Naive Bayes + Random Forest).
#
# Variáveis de ambiente opcionais:
#   DATASET_PATH     - CSV consolidado de treino (default: ml\data\dataset.csv)
#   DATASET_SOURCES  - fontes para o build_dataset, separadas por vírgula
#                      (default: local_csv; ex.: "sting9,kaggle_phishing")
#   USE_SAMPLE_DATA  - "true" gera um dataset sintético e pula o build_dataset
#                      (apenas smoke test; train_model vai abortar por poucos dados)
#   KAGGLE_CSV       - caminho de um CSV do Kaggle já baixado (opcional)
#   STING9_EXPORT    - caminho de um export Sting9 já baixado (opcional)
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$MlDir = Join-Path $RootDir "ml"
$DatasetPath = if ($env:DATASET_PATH) { $env:DATASET_PATH } else { Join-Path $MlDir "data\dataset.csv" }
$DatasetSources = if ($env:DATASET_SOURCES) { $env:DATASET_SOURCES } else { "local_csv" }
$UseSampleData = if ($env:USE_SAMPLE_DATA) { $env:USE_SAMPLE_DATA } else { "false" }

Push-Location $MlDir

if (Test-Path "venv\Scripts\python.exe") {
    $python = "venv\Scripts\python.exe"
} elseif (Test-Path ".venv\Scripts\python.exe") {
    $python = ".venv\Scripts\python.exe"
} else {
    Pop-Location
    throw "Ambiente virtual não encontrado em ml\venv nem ml\.venv. Crie com: python -m venv venv; venv\Scripts\pip install -r requirements.txt"
}

if ($UseSampleData -eq "true") {
    Write-Host "==> Gerando dataset sintético de exemplo (apenas smoke test)..." -ForegroundColor Cyan
    & $python -m training.generate_sample_dataset
} else {
    Write-Host "==> Consolidando dataset a partir das fontes: $DatasetSources ..." -ForegroundColor Cyan
    $buildArgs = @("-m", "training.build_dataset", "--sources", $DatasetSources, "--out", $DatasetPath)
    if ($env:KAGGLE_CSV) { $buildArgs += @("--kaggle-csv", $env:KAGGLE_CSV) }
    if ($env:STING9_EXPORT) { $buildArgs += @("--sting9-export", $env:STING9_EXPORT) }
    & $python @buildArgs
}

Write-Host "==> Treinando modelos (Naive Bayes + Random Forest) com validação cruzada k=10..." -ForegroundColor Cyan
& $python -m training.train_model --dataset $DatasetPath

Write-Host "==> Avaliando modelos treinados..." -ForegroundColor Cyan
& $python -m training.evaluate_model --dataset $DatasetPath

Pop-Location
Write-Host "Concluído. Modelos em ml\models\ | métricas em ml\models\metrics.json e METRICS.md" -ForegroundColor Green
