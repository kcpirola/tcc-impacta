# Inicia os três serviços do projeto (ML, backend, frontend) em janelas separadas.
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot

Write-Host "==> Iniciando microserviço de ML (http://localhost:5000)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\ml'; .\.venv\Scripts\python.exe -m service.app"

Write-Host "==> Iniciando backend .NET (https://localhost:5001)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\backend\PhishingDetectionAPI'; dotnet run"

Write-Host "==> Iniciando frontend React (http://localhost:3000)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\frontend'; npm start"

Write-Host "Todos os serviços foram iniciados em janelas separadas. Feche-as para encerrar." -ForegroundColor Green
