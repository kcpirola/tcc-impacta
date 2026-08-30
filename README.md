# Sistema de Detecção de Phishing em E-mails Corporativos

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Status: Em Desenvolvimento](https://img.shields.io/badge/Status-Em%20Desenvolvimento-blue)]()

Sistema automatizado de detecção e classificação de e-mails de phishing em ambientes corporativos, desenvolvido para a Health2B Hospitalar Ltda.

## 📋 Visão Geral

Este projeto implementa um software baseado em Machine Learning (Naive Bayes + Random Forest) para detectar automaticamente e-mails maliciosos. O sistema classifica e-mails em três categorias: **seguro**, **suspeito** ou **phishing**, com alertas em tempo real e dashboard interativo.

**Características principais:**
- ✅ Classificação automática com 95%+ de acurácia
- ✅ Alertas em tempo real para usuários
- ✅ Dashboard interativo com Power BI
- ✅ Relatórios de segurança automatizados
- ✅ Conformidade com LGPD
- ✅ Sem dependência de serviços em nuvem

---

## 🏗️ Arquitetura

```
Frontend (React)
     ↓
Backend API (C# .NET)
     ↓
Microserviço ML (Python)
     ↓
Banco de Dados (PostgreSQL)
     ↓
Power BI Dashboard
```

**Tecnologias:**
- **Backend:** C# .NET 10
- **Frontend:** React 18+
- **Machine Learning:** Python 3.9+ (scikit-learn)
- **Banco de Dados:** PostgreSQL 13+
- **Relatórios:** Power BI
- **Algoritmos:** Naive Bayes + Random Forest

---

## 📦 Pré-requisitos

### Sistema
- Windows Server 2019+ ou Linux (Ubuntu 20.04+)
- 8GB RAM mínimo
- 50GB espaço em disco
- Acesso ao servidor de e-mail corporativo (IMAP/SMTP)
- Acesso à rede corporativa

### Software Necessário
- .NET 6+ SDK
- Python 3.9+
- PostgreSQL 13+
- Node.js 16+
- Power BI Desktop (para development)
- Git

### Credenciais Necessárias
- Credenciais do servidor de e-mail
- Credenciais do banco de dados
- Chaves API (Sting9 e Kaggle - opcional)

---

## 🚀 Instalação Rápida (Windows Server)

### 1. Preparação do Ambiente

```powershell
# Como Administrador
# Instalar Chocolatey (se não tiver)
Set-ExecutionPolicy Bypass -Scope Process -Force; `
  iex ((New-Object System.Net.ServicePointManager).ServerCertificateValidationCallback = {$true}); `
  iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# Instalar dependências
choco install git python nodejs postgresql --yes

# Verificar instalações
git --version
python --version
node --version
psql --version
```

### 2. Clonar Repositório

```powershell
cd C:\Desenvolvimento
git clone https://seu-repositorio/tcc-impacta.git
cd tcc-impacta
```

### 3. Configurar Banco de Dados

```powershell
# Criar banco de dados
psql -U postgres -c "CREATE DATABASE phishing_detection;"
psql -U postgres -c "CREATE USER app_user WITH ENCRYPTED PASSWORD 'senha_segura_123';"
psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE phishing_detection TO app_user;"

# Executar migrations
cd database
psql -U app_user -d phishing_detection -f schema.sql
psql -U app_user -d phishing_detection -f seed.sql
```

### 4. Configurar Backend (C# .NET)

```powershell
cd backend/PhishingDetectionAPI

# Restaurar dependências
dotnet restore

# Configurar variáveis de ambiente
Copy-Item appsettings.example.json appsettings.json
# Editar appsettings.json com credenciais reais

# Executar migrations EF
dotnet ef database update

# Compilar
dotnet build

# Testar
dotnet test
```

### 5. Configurar Machine Learning (Python)

```powershell
cd ..\..\ml

# Criar ambiente virtual
python -m venv venv
.\venv\Scripts\Activate.ps1

# Instalar dependências
pip install -r requirements.txt

# Treinar modelos (primeira vez)
python training/train_model.py

# Iniciar serviço ML
python service/app.py
# Será executado em: http://localhost:5000
```

### 6. Configurar Frontend (React)

```powershell
cd ..\..\frontend

# Instalar dependências
npm install

# Configurar variáveis de ambiente
Copy-Item .env.example .env.local
# Editar .env.local com URL da API

# Executar em desenvolvimento
npm start
# Será executado em: http://localhost:3000
```

### 7. Iniciar Todos os Serviços

```powershell
# Terminal 1: Backend
cd C:\Desenvolvimento\tcc-impacta\backend\PhishingDetectionAPI
dotnet run

# Terminal 2: ML Service
cd C:\Desenvolvimento\tcc-impacta\ml
.\venv\Scripts\Activate.ps1
python service/app.py

# Terminal 3: Frontend
cd C:\Desenvolvimento\tcc-impacta\frontend
npm start
```

---

## 🔧 Instalação no Servidor da Empresa

### Pré-requisitos para Produção

1. **Servidor Dedicado**
   - Windows Server 2019+ ou Linux
   - 8GB RAM
   - SSD com 100GB+
   - Conexão com servidor de e-mail corporativo

2. **Rede**
   - Acesso ao servidor IMAP/SMTP
   - Firewall configurado para portas: 443, 5000, 3000
   - Certificado SSL/TLS válido

3. **Dados**
   - Backup do banco de dados
   - Credenciais seguras armazenadas em vault

### Passo 1: Preparação do Servidor

```powershell
# Como Administrador
# 1. Atualizar Windows
# 2. Instalar .NET 6+ Runtime
# 3. Instalar PostgreSQL (ou conectar a servidor existente)
# 4. Instalar Python 3.9+
# 5. Configurar firewall

$firewallRules = @(
    @{Port = 443; Name = "HTTPS"},
    @{Port = 5000; Name = "ML_Service"},
    @{Port = 3000; Name = "Frontend"}
)

foreach ($rule in $firewallRules) {
    New-NetFirewallRule -DisplayName $rule.Name `
        -Direction Inbound -Action Allow `
        -Protocol TCP -LocalPort $rule.Port
}
```

### Passo 2: Clonar e Configurar Aplicação

```powershell
# Clone o repositório
cd C:\Producao
git clone https://seu-repositorio/tcc-impacta.git
cd tcc-impacta

# Configure variáveis de ambiente de produção
# Edite os arquivos .env com as credenciais reais:
# - Servidor de e-mail
# - Banco de dados
# - Certificados SSL
# - Chaves de segurança
```

### Passo 3: Configurar Banco de Dados Corporativo

```powershell
# Usar database existente ou criar nova
psql -U postgres -h seu-servidor-db

CREATE DATABASE phishing_detection_prod;
CREATE USER app_prod WITH ENCRYPTED PASSWORD 'senha_muito_segura_xyz';
GRANT ALL PRIVILEGES ON DATABASE phishing_detection_prod TO app_prod;

# Executar schema
psql -U app_prod -h seu-servidor-db -d phishing_detection_prod -f database/schema.sql
```

### Passo 4: Preparar Modelos ML

```powershell
cd ml

# Copiar modelos treinados para produção
Copy-Item models/ C:\Producao\phishing-detection\ml\models\

# OU treinar com dados corporativos
python training/train_model.py --dataset corporate --output models/
```

### Passo 5: Compilar para Produção

```powershell
# Backend
cd backend/PhishingDetectionAPI
dotnet publish -c Release -o "C:\Producao\PhishingDetectionAPI"

# Frontend
cd ..\..\frontend
npm run build
# Copiar pasta 'build' para servidor web (IIS/Nginx)
```

### Passo 6: Configurar Windows Service (Backend)

```powershell
# Criar Windows Service para Backend
$serviceName = "PhishingDetectionAPI"
$appPath = "C:\Producao\PhishingDetectionAPI\PhishingDetectionAPI.exe"

# Usando NSSM (recomendado)
# Baixar: https://nssm.cc/download

nssm install $serviceName $appPath
nssm set $serviceName AppDirectory "C:\Producao\PhishingDetectionAPI"
nssm set $serviceName AppStdout "C:\Producao\logs\backend.log"
nssm set $serviceName AppStderr "C:\Producao\logs\backend_error.log"

# Iniciar serviço
Start-Service $serviceName
```

### Passo 7: Configurar Serviço ML

```powershell
# Criar Windows Service para Python ML Service
$mlServiceName = "PhishingML"
$pythonPath = "C:\Python39\python.exe"
$scriptPath = "C:\Producao\ml\service\app.py"

nssm install $mlServiceName $pythonPath
nssm set $mlServiceName AppParameters $scriptPath
nssm set $mlServiceName AppDirectory "C:\Producao\ml"
nssm set $mlServiceName AppStdout "C:\Producao\logs\ml.log"

Start-Service $mlServiceName
```

### Passo 8: Configurar IIS/Nginx (Frontend)

#### Opção A: IIS (Windows Server)

```powershell
# Instalar Application Request Routing (ARR)
# Acessar: Server Manager > Add Roles and Features > Web Server > Application Development > Application Request Routing

# Configurar Application Pool
New-WebAppPool -Name PhishingUI
Set-WebAppPoolProperty -Name PhishingUI -ManagedPipelineMode Integrated -ManagedRuntimeVersion v4.0

# Copiar aplicação React
Copy-Item "C:\Desenvolvimento\tcc-impacta\frontend\build" "C:\inetpub\wwwroot\phishing-ui"

# Criar Site IIS
New-Website -Name "Phishing Detection UI" `
    -PhysicalPath "C:\inetpub\wwwroot\phishing-ui" `
    -Port 443 -Protocol https `
    -HostHeader "seu-dominio.com"
```

#### Opção B: Nginx (Linux/Windows WSL2)

```nginx
server {
    listen 443 ssl;
    server_name seu-dominio.com;

    ssl_certificate /etc/ssl/certs/seu-certificado.crt;
    ssl_certificate_key /etc/ssl/private/seu-certificado.key;

    root /var/www/phishing-ui/build;
    index index.html index.htm;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://localhost:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    location /ml/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
    }
}
```

### Passo 9: Configurar Certificado SSL

```powershell
# Usar certificado da empresa ou Let's Encrypt

# Option 1: Certificado instalado no servidor
# Importar certificado para IIS/Nginx

# Option 2: Let's Encrypt (para testes)
# Usar Certbot para gerar certificado gratuito
```

### Passo 10: Iniciar Serviços

```powershell
# Verificar status dos serviços
Get-Service "PhishingDetectionAPI", "PhishingML" | Select-Object Name, Status

# Se não estiverem rodando, iniciar
Start-Service "PhishingDetectionAPI"
Start-Service "PhishingML"

# Verificar logs
Get-Content "C:\Producao\logs\backend.log" -Tail 50
Get-Content "C:\Producao\logs\ml.log" -Tail 50
```

---

## ⚙️ Configuração de Integração com E-mail

### 1. Configurar IMAP (Recebimento)

Edite `backend/appsettings.json`:

```json
{
  "EmailSettings": {
    "Host": "seu-servidor-imap.com",
    "Port": 993,
    "Username": "phishing-detector@empresa.com",
    "Password": "senha_segura",
    "UseSsl": true,
    "Mailbox": "INBOX",
    "CheckIntervalSeconds": 60
  }
}
```

### 2. Configurar SMTP (Notificações)

```json
{
  "SmtpSettings": {
    "Host": "seu-servidor-smtp.com",
    "Port": 587,
    "Username": "notifications@empresa.com",
    "Password": "senha_segura",
    "UseTls": true,
    "SenderEmail": "sistema-phishing@empresa.com",
    "SenderName": "Sistema de Detecção de Phishing"
  }
}
```

### 3. Testar Integração

```powershell
# Enviar e-mail de teste
curl -X POST http://localhost:5001/api/email/test `
  -H "Content-Type: application/json" `
  -d '{"emailAddress":"seu-email@empresa.com"}'
```

---

## 📊 Configurar Power BI

### 1. Conectar ao Banco de Dados

1. Abra Power BI Desktop
2. Get Data > PostgreSQL
3. Server: `seu-servidor-db`
4. Database: `phishing_detection_prod`
5. Username: `app_prod`

### 2. Importar Modelo de Dados

```powershell
# Copiar arquivo do modelo
Copy-Item "powerbi\dashboard_template.pbix" "C:\Users\seu-usuario\Documents\Power BI Projects\"
```

### 3. Publicar no Power BI Service

1. File > Publish
2. Selecionar workspace da empresa
3. Configurar refresh automático (diário)

---

## 🔐 Segurança e LGPD

### Checklist de Segurança

- [ ] Certificado SSL/TLS instalado e válido
- [ ] Firewall configurado corretamente
- [ ] Senhas armazenadas em vault seguro (Azure Key Vault, Hashicorp Vault)
- [ ] Acesso ao banco de dados restrito (IP whitelist)
- [ ] Logs de auditoria habilitados
- [ ] Backup automático do banco de dados (diário)
- [ ] Política de retenção de dados definida (90 dias)
- [ ] Consentimento dos usuários coletado (LGPD)
- [ ] Endpoint de exclusão de dados implementado

### Implementar Key Vault (Azure)

```powershell
# Criar Key Vault
az keyvault create --name phishing-detection-vault `
    --resource-group sua-rg `
    --location eastus

# Armazenar segredos
az keyvault secret set --vault-name phishing-detection-vault `
    --name DbPassword --value "senha_segura_123"

az keyvault secret set --vault-name phishing-detection-vault `
    --name EmailPassword --value "email_segura_456"
```

---

## 📋 Monitoramento e Manutenção

### Verificar Status do Sistema

```powershell
# Verificar serviços
Get-Service | Where-Object {$_.Name -like "*Phishing*"}

# Verificar portas abertas
netstat -ano | findstr :5001
netstat -ano | findstr :5000
netstat -ano | findstr :3000

# Verificar conexão com banco
sqlcmd -S seu-servidor-db -U app_prod -P senha -Q "SELECT 1"
```

### Logs Importantes

```powershell
# Backend
Get-Content "C:\Producao\logs\backend.log" -Tail 100

# ML Service
Get-Content "C:\Producao\logs\ml.log" -Tail 100

# Eventos Windows
Get-EventLog -LogName Application -Source "*Phishing*" -Newest 20
```

### Backup do Banco de Dados

```powershell
# Backup manual
$backupPath = "C:\Backups\phishing_detection_$(Get-Date -Format yyyyMMdd_HHmmss).bak"

pg_dump -U app_prod -h seu-servidor-db -d phishing_detection_prod > $backupPath

# Agendar backup automático (Task Scheduler)
# Executar diariamente às 02:00 da manhã
```

---

## 🐛 Troubleshooting

### Serviço ML não inicia

```powershell
# Verificar logs
Get-Content "C:\Producao\logs\ml.log"

# Verificar dependências Python
cd C:\Producao\ml
python -m pip check

# Reinstalar dependências
pip install -r requirements.txt --force-reinstall
```

### Backend não conecta ao banco

```powershell
# Testar conexão
$connectionString = "Host=seu-servidor-db;Database=phishing_detection_prod;Username=app_prod;Password=senha"

# Usar psql para testar
psql -c $connectionString -c "SELECT 1"
```

### Frontend não carrega

```powershell
# Verificar IIS
Get-WebAppPool | Where-Object {$_.Name -eq "PhishingUI"} | Select-Object Name, State

# Reiniciar Application Pool
Restart-WebAppPool -Name PhishingUI

# Verificar event logs
Get-EventLog -LogName Application -Source "IIS" -Newest 20
```

---

## 📞 Suporte e Documentação

### Documentação Adicional
- [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md) - Roadmap do projeto
- [docs/ARQUITETURA.md](docs/ARQUITETURA.md) - Detalhes técnicos
- [docs/API_SPEC.md](docs/API_SPEC.md) - Especificação da API
- [docs/USUARIO_GUIDE.md](docs/USUARIO_GUIDE.md) - Guia do usuário

### Contato Técnico
Para questões técnicas, consulte a documentação ou entre em contato com o time de desenvolvimento.

---

## 📄 Licença

Este projeto é confidencial e proposto para Health2B Hospitalar Ltda.

---

## ✅ Checklist de Deployment

- [ ] Banco de dados criado e migrations executadas
- [ ] Backend compilado e testado
- [ ] ML models treinados e salvos
- [ ] Frontend build criado
- [ ] Certificado SSL instalado
- [ ] Credenciais de e-mail configuradas
- [ ] Power BI conectado
- [ ] Windows Services criados
- [ ] Firewall configurado
- [ ] Backup automático agendado
- [ ] Testes de integração passou
- [ ] Documentação atualizada
- [ ] Usuários treinados
- [ ] Go-live aprovado

---

**Versão:** 1.0  
**Data:** 30 de Agosto de 2026  
**Status:** Pronto para Desenvolvimento
