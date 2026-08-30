# Backend Phishing Detection API

Aplicação backend para detecção de phishing em e-mails corporativos, desenvolvida em **.NET 10** com **C#**, utilizando **PostgreSQL** como banco de dados e seguindo princípios **SOLID** e boas práticas de código limpo.

## 🏗️ Arquitetura

A aplicação segue uma **arquitetura em camadas**:

```
PhishingDetectionAPI/
├── Domain/                          # Camada de domínio (entidades e interfaces)
│   ├── Entities/                   # Modelos de domínio
│   │   ├── BaseEntity.cs          # Classe base para todas as entidades
│   │   ├── User.cs                # Usuário e autenticação
│   │   ├── Email.cs               # E-mail classificado
│   │   ├── EmailRelated.cs        # PhishingIndicator, UrlDetected, Attachment
│   │   └── AuditLog.cs            # Log de auditoria (LGPD)
│   └── Interfaces/                # Contratos de repositórios e serviços
│       ├── IRepository.cs         # Interface genérica e específicas
│       └── IServices.cs           # Interfaces de serviços

├── Application/                     # Camada de aplicação
│   ├── Dtos/                       # Data Transfer Objects
│   │   ├── AuthDto.cs            # Login e autenticação
│   │   └── EmailDto.cs           # Classificação e feedback
│   └── Services/                   # Serviços de negócio
│       └── ApplicationServices.cs # Implementações dos serviços

├── Infrastructure/                  # Camada de infraestrutura
│   ├── Data/
│   │   ├── PhishingDetectionContext.cs  # DbContext do EF Core
│   │   └── Repositories/               # Implementações de repositórios
│   ├── Security/
│   │   └── JwtSettings.cs          # Configurações de JWT
│   └── Extensions/
│       └── InfrastructureExtensions.cs  # Registro de serviços

├── Presentation/                    # Camada de apresentação
│   └── Controllers/                # Controllers da API REST
│       ├── AuthController.cs       # Autenticação
│       └── EmailsController.cs     # Classificação de e-mails

├── Program.cs                      # Configuração principal
├── appsettings.json               # Configurações da aplicação
└── PhishingDetectionAPI.csproj    # Arquivo de projeto .NET
```

## 📋 Tecnologias Utilizadas

| Componente | Versão | Descrição |
|-----------|--------|-----------|
| **.NET** | 10.0 | Framework principal |
| **C#** | 12+ | Linguagem de programação |
| **Entity Framework Core** | 8.0.0 | ORM para acesso a dados |
| **PostgreSQL** | 13+ | Banco de dados relacional |
| **AutoMapper** | 13.0.1 | Mapeamento de objetos (DTO ↔ Entidade) |
| **Serilog** | 3.1.1 | Logging estruturado |
| **FluentValidation** | 11.8.1 | Validação declarativa de dados |
| **JWT** | System.IdentityModel.Tokens.Jwt | Autenticação baseada em tokens |
| **MailKit** | 4.4.0 | Integração com IMAP/SMTP |
| **Refit** | 7.0.0 | Cliente HTTP tipo-seguro |
| **xUnit** | 2.6.6 | Framework de testes |
| **Moq** | 4.20.70 | Mocking para testes unitários |

## 🏛️ Princípios SOLID Aplicados

### 1. **Single Responsibility Principle (SRP)**
Cada classe tem uma única responsabilidade:
- `User.cs`: Representa dados do usuário
- `AuthenticationService.cs`: Gerencia autenticação e JWT
- `UserService.cs`: Gerencia operações de usuário
- `AuthController.cs`: Expõe endpoints de autenticação

### 2. **Open/Closed Principle (OCP)**
Aberto para extensão, fechado para modificação:
- `BaseEntity.cs`: Classe base extensível com virtual `IsValid()`
- `EmailClassification` enum: Novos tipos podem ser adicionados
- `IRepository<T>` genérico: Suporta qualquer entidade derivada

### 3. **Liskov Substitution Principle (LSP)**
Derivações respeitam o contrato da base:
- `User` e `Email` herdam de `BaseEntity` sem quebrar o contrato
- Repositórios específicos (`UserRepository`, `EmailRepository`) estendem `Repository<T>` mantendo consistência

### 4. **Interface Segregation Principle (ISP)**
Interfaces específicas em vez de gerais:
- `IUserRepository`: Operações específicas para usuários
- `IEmailRepository`: Operações específicas para e-mails
- `IAuditLogRepository`: Operações específicas para auditoria
- `IAuthenticationService`, `IUserService`, etc.: Cada serviço tem sua interface

### 5. **Dependency Inversion Principle (DIP)**
Depender de abstrações, não de implementações concretas:
- Controllers injetam `IAuthenticationService`, não `AuthenticationService`
- Serviços injetam `IUserRepository`, não `UserRepository`
- Configuração centralizada em `InfrastructureExtensions.AddInfrastructure()`

## 🔐 Segurança

### Autenticação
- **JWT (JSON Web Token)** para stateless authentication
- Tokens com expiração configurável
- Claims contêm: UserId, Username, Email, Role
- Validação de token antes de cada requisição

### Autorização
- **Role-Based Access Control (RBAC)**: User, Analyst, Admin
- Atributo `[Authorize]` protege endpoints
- Atributo `[AllowAnonymous]` para endpoints públicos

### Dados Sensíveis
- Senhas armazenadas como hash (BCrypt em produção)
- Nenhuma senha em arquivo de configuração
- Variáveis de ambiente para secrets (appsettings.json é exemplo)
- `.gitignore` exclui arquivos sensíveis

### Conformidade LGPD
- Campo `HasLgpdConsent` em User
- Timestamp de criação e atualização em todas as entidades
- `AuditLog` registra todas as ações (rastreabilidade)
- Estrutura para retenção de 90 dias

## 🚀 Como Executar

### Pré-requisitos
- **.NET 10 SDK** instalado
- **PostgreSQL 13+** em execução
- **Banco de dados "phishing_detection"** criado

### 1. Configuração Inicial

```bash
# Clonar repositório
git clone <repo-url>
cd backend/PhishingDetectionAPI

# Restaurar dependências
dotnet restore

# Criar banco de dados (executar script SQL)
# Arquivo: database/schema.sql
psql -U postgres -d postgres -f ../../database/schema.sql
```

### 2. Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=phishing_detection;Username=postgres;Password=sua_senha;"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-com-pelo-menos-32-caracteres",
    "ExpirationMinutes": 60
  }
}
```

### 3. Executar Migrations (Entity Framework)

```bash
dotnet ef database update
```

### 4. Executar a Aplicação

```bash
# Em desenvolvimento
dotnet run

# A API estará disponível em: https://localhost:5001
# Swagger UI em: https://localhost:5001/swagger
```

## 📚 Endpoints da API

### Autenticação (`/api/auth`)

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/login` | Autentica usuário | ❌ Não requerida |
| POST | `/validate-token` | Valida token JWT | ❌ Não requerida |
| GET | `/me` | Retorna usuário autenticado | ✅ Requerida |

### E-mails (`/api/emails`)

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/classify` | Classifica um e-mail | ✅ Requerida |
| POST | `/classify-batch` | Classifica múltiplos e-mails | ✅ Requerida |
| POST | `/feedback` | Envia feedback sobre classificação | ✅ Requerida |
| GET | `/by-classification/{type}` | Lista e-mails por classificação | ✅ Requerida |

## 🧪 Testes

Os testes ficam em um projeto separado, [PhishingDetectionAPI.Tests](../backend/PhishingDetectionAPI.Tests),
para manter xUnit/Moq fora do projeto Web publicado em produção.

```bash
cd backend
dotnet test
```

### Estrutura

```
PhishingDetectionAPI.Tests/
├── Unit/
│   ├── Services/          # AuthenticationService, UserService, EmailClassificationService, AuditService
│   └── Repositories/      # Repository<T>, UserRepository, EmailRepository (EF Core InMemory)
└── Integration/
    ├── CustomWebApplicationFactory.cs   # Sobe a API real com DB InMemory + stub do serviço de ML
    ├── FakeMlServiceClient.cs           # Substitui o IMlServiceClient nos testes
    ├── AuthControllerTests.cs           # POST /api/auth/login
    └── EmailsControllerTests.cs         # POST /api/emails/classify (autenticado)
```

- **Unit**: isola serviços com Moq (repositórios, `IMlServiceClient`) e repositórios com o provider InMemory do EF Core.
- **Integration**: usa `WebApplicationFactory<Program>` para exercitar o pipeline HTTP completo (autenticação JWT, autorização, model binding), substituindo apenas o PostgreSQL (por InMemory) e o microserviço Python (por um stub).

## 🔄 Fluxo de Classificação de E-mail

```
Cliente
   ↓
[POST /api/emails/classify]
   ↓
EmailsController.ClassifyEmail()
   ↓
IEmailClassificationService.ClassifyEmailAsync()
   ↓
[CHAMAR SERVIÇO PYTHON DE ML]
   ↓
ClassificationResponse (safe/suspicious/phishing)
   ↓
[REGISTRAR EM AUDITORIA]
   ↓
Responder JSON ao cliente
```

## 📊 Modelo de Dados

### User
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    department VARCHAR(100),
    role INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    last_login TIMESTAMP NULL,
    has_lgpd_consent BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Email
```sql
CREATE TABLE emails (
    id SERIAL PRIMARY KEY,
    message_id VARCHAR(255) UNIQUE,
    subject VARCHAR(500),
    sender VARCHAR(255) NOT NULL,
    recipients VARCHAR(1000),
    received_date TIMESTAMP NOT NULL,
    processed_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    email_body TEXT,
    email_headers TEXT,
    classification INTEGER DEFAULT 0,
    confidence DECIMAL(5,2),
    is_validated BOOLEAN DEFAULT FALSE,
    user_feedback INTEGER NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## 🐛 Tratamento de Erros

A aplicação retorna respostas padronizadas:

```json
// Sucesso
HTTP/1.1 200 OK
{
  "classification": "phishing",
  "confidence": 87.5,
  "recommendation": "Este e-mail é provavelmente phishing. Não clique em links."
}

// Erro
HTTP/1.1 401 Unauthorized
{
  "message": "Credenciais incorretas"
}

// Erro de validação
HTTP/1.1 400 Bad Request
{
  "message": "Subject, Body e Sender são obrigatórios"
}
```

## 📝 Logging

Serilog configurado para:
- **Console**: Saída em tempo real
- **Arquivo**: `logs/log-YYYY-MM-DD.txt` (rolling daily)
- **Nível mínimo**: Information
- **Estruturado**: JSON para fácil parsing

```csharp
_logger.LogInformation("E-mail classificado como {Classification}", result.Classification);
```

## 🔄 Ambiente de Desenvolvimento

### Arquivo .env (não incluído no repo)
```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=phishing_detection;Username=postgres;Password=dev_password;
JwtSettings__SecretKey=dev-super-secret-key-must-be-at-least-32-characters-long
```

### Variáveis de Ambiente
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

## 📖 Documentação Adicional

- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication em ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Clean Code](https://clean-code-dotnet.com/)

## 🤝 Contribuições

Ver [CONTRIBUINDO.md](../../CONTRIBUINDO.md) para diretrizes de contribuição.

## ⚖️ Licença

Este projeto é parte de um TCC da Faculdade Impacta.

---

**Última atualização**: Janeiro 2025  
**Versão**: 1.0.0  
**Estatus**: Em desenvolvimento
