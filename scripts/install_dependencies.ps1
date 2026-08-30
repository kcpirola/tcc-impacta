# Instala as dependências dos três módulos do projeto (backend, ML, frontend).
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot

Write-Host "==> Instalando dependências do backend (.NET)..." -ForegroundColor Cyan
Push-Location "$RootDir\backend\PhishingDetectionAPI"
dotnet restore
Pop-Location
Push-Location "$RootDir\backend\PhishingDetectionAPI.Tests"
dotnet restore
Pop-Location

Write-Host "==> Instalando dependências do microserviço de ML (Python)..." -ForegroundColor Cyan
Push-Location "$RootDir\ml"
if (-not (Test-Path ".venv")) {
    py -m venv .venv
}
& ".venv\Scripts\python.exe" -m pip install --upgrade pip
& ".venv\Scripts\python.exe" -m pip install -r requirements.txt
Pop-Location

Write-Host "==> Instalando dependências do frontend (React)..." -ForegroundColor Cyan
Push-Location "$RootDir\frontend"
npm install
Pop-Location

Write-Host "Todas as dependências foram instaladas com sucesso." -ForegroundColor Green
