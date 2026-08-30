#!/usr/bin/env bash
# Instala as dependências dos três módulos do projeto (backend, ML, frontend).
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "==> Instalando dependências do backend (.NET)..."
(cd "$ROOT_DIR/backend/PhishingDetectionAPI" && dotnet restore)
(cd "$ROOT_DIR/backend/PhishingDetectionAPI.Tests" && dotnet restore)

echo "==> Instalando dependências do microserviço de ML (Python)..."
(
    cd "$ROOT_DIR/ml"
    if [ ! -d ".venv" ]; then
        python3 -m venv .venv
    fi
    source .venv/bin/activate
    pip install --upgrade pip
    pip install -r requirements.txt
    deactivate
)

echo "==> Instalando dependências do frontend (React)..."
(cd "$ROOT_DIR/frontend" && npm install)

echo "Todas as dependências foram instaladas com sucesso."
