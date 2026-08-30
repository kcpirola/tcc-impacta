#!/usr/bin/env bash
# Cria o banco de dados, o usuário de aplicação e aplica schema.sql + seed.sql.
# Variáveis de ambiente (todas opcionais, com defaults para desenvolvimento local):
#   PGADMIN_USER, DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD, SEED_SAMPLE_DATA
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

ADMIN_USER="${PGADMIN_USER:-postgres}"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-phishing_detection}"
DB_USER="${DB_USER:-app_user}"
DB_PASSWORD="${DB_PASSWORD:-changeme}"
SEED_SAMPLE_DATA="${SEED_SAMPLE_DATA:-false}"

echo "==> Criando banco de dados '$DB_NAME' (se não existir)..."
psql -U "$ADMIN_USER" -h "$DB_HOST" -p "$DB_PORT" -tc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'" | grep -q 1 || \
    psql -U "$ADMIN_USER" -h "$DB_HOST" -p "$DB_PORT" -c "CREATE DATABASE $DB_NAME;"

echo "==> Criando usuário de aplicação '$DB_USER' (se não existir)..."
psql -U "$ADMIN_USER" -h "$DB_HOST" -p "$DB_PORT" -tc "SELECT 1 FROM pg_roles WHERE rolname = '$DB_USER'" | grep -q 1 || \
    psql -U "$ADMIN_USER" -h "$DB_HOST" -p "$DB_PORT" -c "CREATE USER $DB_USER WITH ENCRYPTED PASSWORD '$DB_PASSWORD';"

psql -U "$ADMIN_USER" -h "$DB_HOST" -p "$DB_PORT" -c "GRANT ALL PRIVILEGES ON DATABASE $DB_NAME TO $DB_USER;"

echo "==> Aplicando database/schema.sql..."
psql -U "$DB_USER" -h "$DB_HOST" -p "$DB_PORT" -d "$DB_NAME" -f "$ROOT_DIR/database/schema.sql"

echo "==> Aplicando database/seed.sql..."
if [ "$SEED_SAMPLE_DATA" = "true" ]; then
    PGOPTIONS="-c phishing_detection.seed_sample_data=true" \
        psql -U "$DB_USER" -h "$DB_HOST" -p "$DB_PORT" -d "$DB_NAME" -f "$ROOT_DIR/database/seed.sql"
else
    psql -U "$DB_USER" -h "$DB_HOST" -p "$DB_PORT" -d "$DB_NAME" -f "$ROOT_DIR/database/seed.sql"
fi

echo "Banco de dados configurado com sucesso. Lembre-se de trocar a senha padrão do usuário admin."
