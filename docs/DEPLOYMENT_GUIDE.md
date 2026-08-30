# Guia de Deployment - Sistema de Detecção de Phishing

## Documento de Procedimentos para Instalação no Servidor da Health2B Hospitalar Ltda.

**Data:** 30 de Agosto de 2026  
**Versão:** 1.0  
**Status:** Pronto para Implementação

---

## 📋 Checklist Pré-Deployment

### Hardware
- [ ] Servidor Windows Server 2019+ ou Linux (Ubuntu 20.04+)
- [ ] 8GB RAM mínimo (16GB recomendado)
- [ ] 100GB SSD disponível
- [ ] Conexão de rede estável (100 Mbps mínimo)
- [ ] Acesso administrativo ao servidor

### Rede e Segurança
- [ ] Firewall configurado para portas: 443, 5001, 5000, 3000
- [ ] Certificado SSL/TLS válido (self-signed ou corporativo)
- [ ] Acesso ao servidor IMAP/SMTP da empresa
- [ ] Proxy/VPN configurado (se necessário)
- [ ] IP whitelist liberado

### Credenciais e Dados
- [ ] Credenciais do servidor de e-mail
- [ ] Credenciais do banco PostgreSQL
- [ ] Chaves de segurança geradas
- [ ] Senhas armazenadas em vault seguro

### Aprovações
- [ ] Aprovação do CIO/Gerente de TI
- [ ] Aprovação de Segurança
- [ ] Plano de rollback documentado
- [ ] Janela de manutenção agendada

---

## 🔧 Passo 1: Preparação do Servidor (Dia 1)

### 1.1 Atualizar Sistema Operacional

**Windows Server:**
```powershell
# Como Administrador
# Abrir Windows Update
Start-Process ms-settings:windowsupdate

# Aplicar todos os updates
Get-WindowsUpdate | Install-WindowsUpdate -AcceptAll

# Reiniciar se necessário
Restart-Computer -Force
```

**Linux (Ubuntu):**
```bash
sudo apt update
sudo apt full-upgrade -y
sudo reboot
```

### 1.2 Instalar Pré-requisitos

**Windows Server:**
```powershell
# Instalar Chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Instalar dependências
choco install dotnet-sdk python nodejs postgresql-server git -y

# Verificar instalações
dotnet --version
python --version
node --version
psql --version
git --version
```

**Linux (Ubuntu):**
```bash
# Instalar .NET 6+
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 6.0 --install-dir /usr/local/dotnet
export PATH=$PATH:/usr/local/dotnet

# Instalar Python
sudo apt install python3.9 python3-pip -y

# Instalar Node.js
curl -fsSL https://deb.nodesource.com/setup_16.x | sudo -E bash -
sudo apt-get install -y nodejs

# Instalar PostgreSQL
sudo apt install postgresql postgresql-contrib -y

# Instalar Git
sudo apt install git -y
```

### 1.3 Criar Estrutura de Diretórios

**Windows:**
```powershell
$basePath = "C:\Producao"

# Criar diretórios
New-Item -ItemType Directory -Path "$basePath\phishing-detection" -Force
New-Item -ItemType Directory -Path "$basePath\logs" -Force
New-Item -ItemType Directory -Path "$basePath\backups" -Force
New-Item -ItemType Directory -Path "$basePath\certificates" -Force
New-Item -ItemType Directory -Path "$basePath\config" -Force

# Definir permissões
$acl = Get-Acl "$basePath"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "NETWORK SERVICE", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.AddAccessRule($rule)
Set-Acl "$basePath" $acl
```

**Linux:**
```bash
sudo mkdir -p /var/www/phishing-detection
sudo mkdir -p /var/log/phishing-detection
sudo mkdir -p /var/backups/phishing-detection
sudo mkdir -p /etc/phishing-detection
sudo chown -R www-data:www-data /var/www/phishing-detection
```

---

## 💾 Passo 2: Configurar Banco de Dados (Dia 1)

### 2.1 Criar Banco PostgreSQL

**Windows (usando pgAdmin ou PowerShell):**
```powershell
# Iniciar PostgreSQL (se não estiver)
Start-Service postgresql-x64-13

# Conectar e criar banco
$env:PGPASSWORD = "sua_senha_postgres"
psql -U postgres -h localhost -f "C:\Producao\phishing-detection\database\schema.sql"

# Criar usuário de aplicação
psql -U postgres -h localhost -c `
  "CREATE USER app_prod WITH ENCRYPTED PASSWORD 'senha_super_segura_prod';"

psql -U postgres -h localhost -c `
  "ALTER ROLE app_prod CREATEDB;"

# Conceder privilégios
psql -U postgres -h localhost -d phishing_detection_prod -c `
  "GRANT ALL PRIVILEGES ON SCHEMA public TO app_prod;"
```

**Linux:**
```bash
# Iniciar PostgreSQL
sudo systemctl start postgresql

# Conectar e criar banco
sudo -u postgres psql << EOF
CREATE DATABASE phishing_detection_prod;
CREATE USER app_prod WITH ENCRYPTED PASSWORD 'senha_super_segura_prod';
ALTER ROLE app_prod WITH CREATEDB;
GRANT ALL PRIVILEGES ON DATABASE phishing_detection_prod TO app_prod;
EOF

# Executar schema
sudo -u postgres psql -d phishing_detection_prod -f /var/www/phishing-detection/database/schema.sql
```

### 2.2 Verificar Conectividade

```powershell
# Windows
$env:PGPASSWORD = "senha_super_segura_prod"
psql -U app_prod -h localhost -d phishing_detection_prod -c "SELECT 1;"

# Linux
sudo -u postgres psql -d phishing_detection_prod -c "SELECT 1;"
```

### 2.3 Configurar Backup Automático

**Windows (Task Scheduler):**
```powershell
# Script: C:\Producao\scripts\backup-database.ps1
$dateTime = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "C:\Producao\backups\phishing_detection_$dateTime.sql"

$env:PGPASSWORD = "senha_super_segura_prod"
pg_dump -U app_prod -h localhost phishing_detection_prod | Out-File $backupPath

# Manter apenas últimos 30 dias
Get-ChildItem "C:\Producao\backups\*" -include *.sql | 
  Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-30)} | 
  Remove-Item

# Criar tarefa agendada
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Producao\scripts\backup-database.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 02:00
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "PhishingDB_Backup" -Description "Backup diário do banco de dados"
```

**Linux (Cron):**
```bash
# Arquivo: /home/postgres/backup-database.sh
#!/bin/bash
BACKUP_DIR="/var/backups/phishing-detection"
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -U app_prod -h localhost phishing_detection_prod > "$BACKUP_DIR/phishing_detection_$DATE.sql"
find "$BACKUP_DIR" -name "*.sql" -mtime +30 -delete

# Tornar executável e adicionar ao crontab
chmod +x /home/postgres/backup-database.sh
# Editar: crontab -e
# Adicionar: 0 2 * * * /home/postgres/backup-database.sh
```

---

## 🎯 Passo 3: Instalar Backend (Dia 2)

### 3.1 Clonar e Preparar Repositório

```powershell
cd C:\Producao
git clone https://seu-repositorio/tcc-impacta.git phishing-detection
cd phishing-detection
git checkout main  # ou release branch
```

### 3.2 Configurar Variáveis de Ambiente

```powershell
# Copiar template
Copy-Item ".env.example" ".env.production"

# Editar com valores reais
notepad .env.production

# Conteúdo esperado:
# ENVIRONMENT=Production
# DB_HOST=localhost
# DB_NAME=phishing_detection_prod
# DB_USER=app_prod
# DB_PASSWORD=senha_super_segura_prod
# EMAIL_IMAP_HOST=seu-servidor-imap.com
# EMAIL_IMAP_USERNAME=detector@empresa.com
# etc...
```

### 3.3 Publicar Aplicação

```powershell
cd backend/PhishingDetectionAPI

# Restaurar dependências
dotnet restore

# Compilar para Production
dotnet publish -c Release -o "C:\Producao\backend-api" --no-self-contained

# Gerar arquivo de configuração
Copy-Item "appsettings.example.json" "C:\Producao\backend-api\appsettings.Production.json"
```

### 3.4 Criar Windows Service

```powershell
# Baixar NSSM: https://nssm.cc/download
# Descompactar em C:\tools\nssm

cd C:\tools\nssm\win64

# Instalar serviço
.\nssm install PhishingDetectionAPI "C:\Producao\backend-api\PhishingDetectionAPI.exe"

# Configurar
.\nssm set PhishingDetectionAPI AppDirectory "C:\Producao\backend-api"
.\nssm set PhishingDetectionAPI AppStdout "C:\Producao\logs\api_stdout.log"
.\nssm set PhishingDetectionAPI AppStderr "C:\Producao\logs\api_stderr.log"
.\nssm set PhishingDetectionAPI AppRotateFiles 1
.\nssm set PhishingDetectionAPI AppRotateOnline 1

# Configurar para iniciar com sistema
.\nssm set PhishingDetectionAPI ObjectName LocalSystem
.\nssm set PhishingDetectionAPI Type service
.\nssm set PhishingDetectionAPI Start SERVICE_AUTO_START

# Iniciar serviço
.\nssm start PhishingDetectionAPI

# Verificar status
Get-Service "PhishingDetectionAPI" | Select-Object Name, Status
```

### 3.5 Testar Backend

```powershell
# Aguardar serviço iniciar (30 segundos)
Start-Sleep -Seconds 30

# Verificar se está respondendo
$response = Invoke-WebRequest -Uri "http://localhost:5001/api/health" -ErrorAction SilentlyContinue
$response.StatusCode  # Deve retornar 200

# Verificar logs
Get-Content "C:\Producao\logs\api_stdout.log" -Tail 50
```

---

## 🐍 Passo 4: Instalar Machine Learning (Dia 2)

### 4.1 Preparar Ambiente Python

```powershell
cd C:\Producao\phishing-detection\ml

# Criar virtual environment
python -m venv venv

# Ativar
.\venv\Scripts\Activate.ps1

# Instalar dependências
pip install --upgrade pip
pip install -r requirements.txt

# Verificar
pip list
```

### 4.2 Treinar Modelos (se primeira execução)

```powershell
# Ainda no virtual environment ativo

# Baixar datasets (opcional, se tiver acesso)
python training/download_datasets.py

# Treinar modelos
python training/train_model.py --dataset combined --output models/

# Avaliar desempenho
python training/evaluate_model.py

# Verificar métricas
# Deve exibir: Acuracy > 95%, Precision > 94%, Recall > 96%, F1 > 95%
```

### 4.3 Criar Serviço Python

```powershell
# Usar NSSM para criar serviço Python
cd C:\tools\nssm\win64

$pythonExe = "C:\Producao\phishing-detection\ml\venv\Scripts\python.exe"
$scriptPath = "C:\Producao\phishing-detection\ml\service\app.py"
$workDir = "C:\Producao\phishing-detection\ml"

.\nssm install PhishingMLService $pythonExe
.\nssm set PhishingMLService AppParameters $scriptPath
.\nssm set PhishingMLService AppDirectory $workDir
.\nssm set PhishingMLService AppStdout "C:\Producao\logs\ml_stdout.log"
.\nssm set PhishingMLService AppStderr "C:\Producao\logs\ml_stderr.log"
.\nssm set PhishingMLService AppRotateFiles 1

.\nssm start PhishingMLService
```

### 4.4 Testar ML Service

```powershell
# Aguardar inicialização
Start-Sleep -Seconds 10

# Teste de classificação
$testEmail = @{
    subject = "Teste"
    body = "Teste de classificação"
    sender = "test@test.com"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/classify" `
    -Method POST `
    -ContentType "application/json" `
    -Body $testEmail

$response.Content | ConvertFrom-Json
# Deve retornar: {"classification": "safe"|"suspicious"|"phishing", "confidence": 0.xx}
```

---

## 🎨 Passo 5: Instalar Frontend (Dia 2)

### 5.1 Build da Aplicação React

```powershell
cd C:\Producao\phishing-detection\frontend

# Instalar dependências
npm install

# Build para produção
npm run build

# Verificar resultado
Get-ChildItem -Path "build" | Measure-Object
```

### 5.2 Configurar IIS

**Instalar Role:**
```powershell
# Como Administrador
Import-Module ServerManager
Add-WindowsFeature Web-Server -IncludeAllSubFeature
Add-WindowsFeature Web-Asp-Net45
```

**Criar Site:**
```powershell
# Criar Application Pool
$appPoolName = "PhishingUI"
$appPool = New-WebAppPool -Name $appPoolName -Force

# Configurar
Set-WebAppPoolProperty -Name $appPoolName -ManagedPipelineMode Integrated
Set-WebAppPoolProperty -Name $appPoolName -ManagedRuntimeVersion v4.0

# Copiar arquivos do build
Copy-Item "C:\Producao\phishing-detection\frontend\build\*" `
    "C:\inetpub\wwwroot\phishing-ui" -Recurse -Force

# Criar Site
$website = New-Website -Name "Phishing Detection" `
    -PhysicalPath "C:\inetpub\wwwroot\phishing-ui" `
    -ApplicationPool $appPoolName `
    -Force

# Configurar Binding (HTTPS)
Remove-WebBinding -Name "Phishing Detection" -Port 80 -Protocol http -Force
New-WebBinding -Name "Phishing Detection" `
    -Port 443 `
    -Protocol https `
    -CertificateThumbprint "seu-certificado-thumbprint"
```

### 5.3 Configurar Reverse Proxy (IIS ARR)

```powershell
# Instalar Application Request Routing
Import-Module WebAdministration
Add-WindowsFeature Web-Url-Rewrite
Add-WindowsFeature Web-Application-Request-Routing

# Criar regra de proxy
Add-WebConfigurationProperty -pspath "MACHINE/WEBROOT/APPHOST" `
    -filter "/system.webServer/rewrite/globalRules" `
    -name "." `
    -value @{name='BackendProxyRule'; patternSyntax='Wildcard'; pattern='*'; ignoreCase='True'} `
    -cloneName "BackendProxyRule"
```

---

## 🔒 Passo 6: Configurar Segurança (Dia 3)

### 6.1 Instalar Certificado SSL/TLS

**Option 1: Certificado Corporativo**
```powershell
# Importar certificado
$cert = Import-PfxCertificate -FilePath "C:\Producao\certificates\seu-certificado.pfx" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -Password (ConvertTo-SecureString "senha-cert" -AsPlainText -Force)

$thumbprint = $cert.Thumbprint

# Vinculary ao site IIS
New-WebBinding -Name "Phishing Detection" `
    -Port 443 `
    -Protocol https `
    -CertificateThumbprint $thumbprint
```

**Option 2: Let's Encrypt (para testes)**
```powershell
# Instalar Certbot
choco install certbot -y

# Gerar certificado
certbot certonly --standalone -d seu-dominio.com

# Renovação automática
certbot renew --quiet --no-eff-email

# Importar para IIS
# Seguir instruções de importação
```

### 6.2 Configurar Firewall

```powershell
# Habilitar inbound rules
$ports = @(
    @{Port = 443; Name = "HTTPS_Incoming"},
    @{Port = 5001; Name = "API_Backend"},
    @{Port = 5000; Name = "ML_Service"}
)

foreach ($rule in $ports) {
    New-NetFirewallRule -DisplayName $rule.Name `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $rule.Port `
        -RemoteAddress "192.168.0.0/16"  # Apenas rede interna
}

# Bloquear tráfego não autorizado
Set-NetFirewallProfile -DefaultInboundAction Block -DefaultOutboundAction Allow
```

### 6.3 Configurar HTTPS Redirect

**Criar arquivo de rewrite em C:\inetpub\wwwroot\phishing-ui\web.config:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="HTTP to HTTPS redirect" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{HTTPS}" pattern="^OFF$" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}{REQUEST_URI}" redirectType="Permanent" />
        </rule>
        <rule name="React Routes" stopProcessing="true">
          <match url="^(?!.*\.(js|json|css|png|jpg|gif|ico|svg)).*$" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_METHOD}" pattern="^GET$" />
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### 6.4 Configurar CORS e Headers de Segurança

**Editar appsettings.Production.json:**
```json
{
  "Cors": {
    "AllowedOrigins": ["https://seu-dominio.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"],
    "AllowCredentials": true,
    "MaxAge": 3600
  },
  "SecurityHeaders": {
    "X-Content-Type-Options": "nosniff",
    "X-Frame-Options": "DENY",
    "X-XSS-Protection": "1; mode=block",
    "Strict-Transport-Security": "max-age=31536000; includeSubDomains",
    "Content-Security-Policy": "default-src 'self'"
  }
}
```

---

## 📊 Passo 7: Configurar Power BI (Dia 3)

### 7.1 Configurar Fonte de Dados

1. Abrir Power BI Desktop
2. Get Data → PostgreSQL
3. Configurar:
   ```
   Server: seu-servidor-db
   Database: phishing_detection_prod
   Username: app_prod
   Password: senha_super_segura_prod
   ```
4. Carregar dados

### 7.2 Criar Dashboard

1. Importar modelo de dashboard: `powerbi/dashboard_template.pbix`
2. Ajustar data sources
3. Publicar no Power BI Service
4. Configurar refresh automático (diário)

### 7.3 Configurar Alertas

1. Power BI Service → Dataset
2. Configurar alertas para:
   - Aumento anômalo de phishing detectado
   - Taxa de false positives elevada
   - Problemas de performance do modelo

---

## ✅ Passo 8: Testes de Integração (Dia 4)

### 8.1 Testar Fluxo Completo

```powershell
# 1. Verificar serviços
Get-Service | Where-Object {$_.Name -like "*Phishing*"}

# 2. Testar Backend
$response = Invoke-RestMethod -Uri "http://localhost:5001/api/health"
$response

# 3. Testar ML
$testData = @{
    subject = "Urgent: Verify Your Account Now"
    body = "Click here to verify your account immediately or it will be closed"
    sender = "noreply@suspicious-bank.com"
} | ConvertTo-Json

$mlResponse = Invoke-RestMethod -Uri "http://localhost:5000/classify" `
    -Method POST -ContentType "application/json" -Body $testData
$mlResponse

# 4. Testar Frontend
Start-Process "https://seu-dominio.com"

# 5. Testar integração com e-mail
# Enviar e-mail de teste real para testar integração IMAP

# 6. Verificar logs
Get-Content "C:\Producao\logs\api_stdout.log" -Tail 100
Get-Content "C:\Producao\logs\ml_stdout.log" -Tail 100
```

### 8.2 Teste de Carga

```powershell
# Usar ferramenta como JMeter ou Wrk
# Simular 100 requisições/segundo durante 5 minutos

# Verificar:
# - Response time: < 2s
# - Error rate: < 1%
# - CPU usage: < 80%
# - Memory: < 85%
```

### 8.3 Teste de Segurança

- [ ] Testar OWASP Top 10
- [ ] Verificar proteção contra SQL Injection
- [ ] Verificar proteção contra XSS
- [ ] Testar autenticação/autorização
- [ ] Verificar HTTPS em todo lugar
- [ ] Validar LGPD compliance

---

## 🚀 Passo 9: Go-Live (Dia 5)

### 9.1 Comunicação

```
Informar usuários:
- Data/hora de implementação
- Funcionalidades disponíveis
- Como reportar problemas
- Documentação disponível
```

### 9.2 Monitoramento Inicial

```powershell
# Monitorar durante as primeiras 24 horas
$monitoringScript = {
    while ($true) {
        Write-Host (Get-Date) "- Verificando serviços..."
        Get-Service | Where-Object {$_.Name -like "*Phishing*"} | Select-Object Name, Status
        
        # Verificar performance
        Get-Process | Where-Object {$_.Name -like "*dotnet*" -or $_.Name -like "*python*"} | 
            Select-Object Name, CPU, WorkingSet
        
        Start-Sleep -Seconds 300  # A cada 5 minutos
    }
}

# Executar script de monitoramento
& $monitoringScript
```

### 9.3 Plano de Rollback

Se problemas críticos:

```powershell
# 1. Parar serviços
Stop-Service "PhishingDetectionAPI"
Stop-Service "PhishingMLService"

# 2. Restaurar do backup
$latestBackup = Get-ChildItem "C:\Producao\backups\*.sql" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
psql -U app_prod -d phishing_detection_prod < $latestBackup.FullName

# 3. Restaurar versão anterior
git checkout main~1
dotnet publish -c Release -o "C:\Producao\backend-api"

# 4. Reiniciar serviços
Start-Service "PhishingDetectionAPI"
Start-Service "PhishingMLService"
```

---

## 📞 Suporte Pós-Deployment

### Contatos de Suporte Técnico
- **Gerente de TI:** [contato]
- **Desenvolvedor Backend:** [contato]
- **Desenvolvedor ML:** [contato]
- **Desenvolvedor Frontend:** [contato]

### Procedimentos de Suporte
1. Verificar logs em `C:\Producao\logs\`
2. Consultar README.md
3. Contatar desenvolvedor relevante
4. Abrir ticket de suporte

### Manutenção Programada
- **Backup:** Diariamente às 02:00
- **Atualização de modelos:** Semanalmente
- **Verificação de logs:** Diariamente
- **Power BI refresh:** Diariamente às 03:00

---

## 📄 Documentação Relacionada

- [README.md](README.md) - Instruções de instalação
- [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md) - Roadmap
- [.env.example](.env.example) - Variáveis de ambiente
- [database/schema.sql](database/schema.sql) - Schema do banco

---

**Documento Finalizado**  
**Versão:** 1.0  
**Data:** 30 de Agosto de 2026
