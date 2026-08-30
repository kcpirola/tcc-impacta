# Cria o banco de dados, o usuário de aplicação e aplica schema.sql + seed.sql.
# Variáveis de ambiente (todas opcionais, com defaults para desenvolvimento local):
#   PGADMIN_USER, DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD, SEED_SAMPLE_DATA
$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot

$AdminUser = if ($env:PGADMIN_USER) { $env:PGADMIN_USER } else { "postgres" }
$DbHost = if ($env:DB_HOST) { $env:DB_HOST } else { "localhost" }
$DbPort = if ($env:DB_PORT) { $env:DB_PORT } else { "5432" }
$DbName = if ($env:DB_NAME) { $env:DB_NAME } else { "phishing_detection" }
$DbUser = if ($env:DB_USER) { $env:DB_USER } else { "app_user" }
$DbPassword = if ($env:DB_PASSWORD) { $env:DB_PASSWORD } else { "changeme" }
$SeedSampleData = if ($env:SEED_SAMPLE_DATA) { $env:SEED_SAMPLE_DATA } else { "false" }

Write-Host "==> Criando banco de dados '$DbName' (se não existir)..." -ForegroundColor Cyan
$dbExists = (psql -U $AdminUser -h $DbHost -p $DbPort -tc "SELECT 1 FROM pg_database WHERE datname = '$DbName'").Trim()
if ($dbExists -ne "1") {
    psql -U $AdminUser -h $DbHost -p $DbPort -c "CREATE DATABASE $DbName;"
}

Write-Host "==> Criando usuário de aplicação '$DbUser' (se não existir)..." -ForegroundColor Cyan
$userExists = (psql -U $AdminUser -h $DbHost -p $DbPort -tc "SELECT 1 FROM pg_roles WHERE rolname = '$DbUser'").Trim()
if ($userExists -ne "1") {
    psql -U $AdminUser -h $DbHost -p $DbPort -c "CREATE USER $DbUser WITH ENCRYPTED PASSWORD '$DbPassword';"
}

psql -U $AdminUser -h $DbHost -p $DbPort -c "GRANT ALL PRIVILEGES ON DATABASE $DbName TO $DbUser;"

Write-Host "==> Aplicando database/schema.sql..." -ForegroundColor Cyan
psql -U $DbUser -h $DbHost -p $DbPort -d $DbName -f "$RootDir\database\schema.sql"

Write-Host "==> Aplicando database/seed.sql..." -ForegroundColor Cyan
if ($SeedSampleData -eq "true") {
    $env:PGOPTIONS = "-c phishing_detection.seed_sample_data=true"
}
psql -U $DbUser -h $DbHost -p $DbPort -d $DbName -f "$RootDir\database\seed.sql"
Remove-Item Env:\PGOPTIONS -ErrorAction SilentlyContinue

Write-Host "Banco de dados configurado com sucesso. Lembre-se de trocar a senha padrão do usuário admin." -ForegroundColor Green
