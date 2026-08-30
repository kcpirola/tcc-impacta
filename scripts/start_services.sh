#!/usr/bin/env bash
# Inicia os três serviços do projeto (ML, backend, frontend) em paralelo.
# Pressione Ctrl+C para encerrar todos os processos.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIDS=()

cleanup() {
    echo ""
    echo "==> Encerrando serviços..."
    for pid in "${PIDS[@]}"; do
        kill "$pid" 2>/dev/null || true
    done
}
trap cleanup EXIT INT TERM

echo "==> Iniciando microserviço de ML (http://localhost:5000)..."
(cd "$ROOT_DIR/ml" && source .venv/bin/activate && python -m service.app) &
PIDS+=("$!")

echo "==> Iniciando backend .NET (https://localhost:5001)..."
(cd "$ROOT_DIR/backend/PhishingDetectionAPI" && dotnet run) &
PIDS+=("$!")

echo "==> Iniciando frontend React (http://localhost:3000)..."
(cd "$ROOT_DIR/frontend" && npm start) &
PIDS+=("$!")

echo "Todos os serviços foram iniciados. Pressione Ctrl+C para encerrar."
wait
