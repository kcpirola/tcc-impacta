# Especificação da API - Sistema de Detecção de Phishing

**Data:** 30 de Agosto de 2026
**Versão:** 1.0
**Base URL (dev):** `https://localhost:5001/api`
**Documentação interativa:** `https://localhost:5001/swagger` (ambiente de desenvolvimento)

---

## 1. Autenticação

A API usa **JWT Bearer Token**. Após o login, envie o token em todas as requisições
protegidas:

```
Authorization: Bearer <token>
```

Tokens expiram após `JwtSettings:ExpirationMinutes` (padrão: 60 minutos). Não há
endpoint de refresh token nesta versão — o cliente deve autenticar novamente.

## 2. Formato de Erros

Respostas de erro seguem o padrão:

```json
{
  "message": "Descrição legível do erro"
}
```

| Código HTTP | Significado |
|-------------|--------------|
| 400 | Dados de entrada inválidos (campos obrigatórios ausentes, formato incorreto) |
| 401 | Não autenticado ou token inválido/expirado |
| 403 | Autenticado, mas sem permissão para o recurso |
| 500 | Erro interno do servidor |
| 503 | Dependência indisponível (ex.: microserviço de ML fora do ar) |

---

## 3. Auth (`/api/auth`)

### POST `/api/auth/login`

Autentica um usuário e retorna um token JWT.

**Autenticação:** não requerida.

**Request:**
```json
{
  "username": "analista01",
  "password": "SenhaForte123!"
}
```

**Response `200 OK`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "analista01",
    "email": "analista01@empresa.com",
    "fullName": "Ana Lista",
    "department": "TI",
    "role": "Analyst",
    "isActive": true,
    "lastLogin": "2026-08-30T10:00:00Z"
  }
}
```

**Erros:** `400` (username/password ausentes), `401` (credenciais incorretas ou usuário inativo).

---

### POST `/api/auth/validate-token`

Valida se um token JWT ainda é válido.

**Autenticação:** não requerida.

**Request:** corpo é a string do token (`"eyJhbGci..."`).

**Response `200 OK`:** `{ "message": "Token é válido" }`
**Response `401`:** `{ "message": "Token é inválido ou expirou" }`

---

### GET `/api/auth/me`

Retorna os dados do usuário autenticado (extraídos do token).

**Autenticação:** requerida.

**Response `200 OK`:**
```json
{ "userId": "1", "username": "analista01", "email": "analista01@empresa.com", "role": "Analyst" }
```

---

## 4. Emails (`/api/emails`)

Todos os endpoints abaixo exigem autenticação (`Authorization: Bearer <token>`).

### POST `/api/emails/classify`

Classifica um único e-mail usando o ensemble Naive Bayes + Random Forest
(via microserviço de ML).

**Request:**
```json
{
  "subject": "URGENTE: Verifique sua conta",
  "body": "Clique aqui: http://192.168.1.1/login",
  "sender": "alert@bank-verify.tk",
  "recipients": "financeiro@empresa.com",
  "headers": null
}
```

| Campo | Obrigatório | Descrição |
|-------|-------------|-----------|
| `subject` | Sim | Assunto do e-mail |
| `body` | Sim | Corpo do e-mail (texto ou HTML) |
| `sender` | Sim | Endereço do remetente |
| `recipients` | Não | Destinatários, separados por `;` |
| `headers` | Não | Headers brutos do e-mail em JSON |

**Response `200 OK`:**
```json
{
  "classification": "phishing",
  "confidence": 92.4,
  "indicators": [
    { "type": "suspicious_url", "description": "1 URL(s) suspeita(s) detectada(s)", "riskScore": 25.0 },
    { "type": "urgent_language", "description": "1 termo(s) de urgência detectado(s)", "riskScore": 20.0 }
  ],
  "suspiciousUrls": ["http://192.168.1.1/login"],
  "recommendation": "Este e-mail é provavelmente phishing. Não clique em links nem forneça dados."
}
```

`classification` é sempre um de: `safe`, `suspicious`, `phishing`.

**Erros:** `400` (campos obrigatórios ausentes), `401`, `500`.

---

### POST `/api/emails/classify-batch`

Classifica múltiplos e-mails em uma única requisição.

**Request:** lista de objetos no mesmo formato de `classify`.

**Response `200 OK`:** lista de objetos no mesmo formato da resposta de `classify`,
na mesma ordem da requisição.

---

### POST `/api/emails/feedback`

Registra o feedback de um analista sobre uma classificação, usado para retroalimentar
o modelo.

**Request:**
```json
{ "emailId": 42, "feedback": "correct" }
```

`feedback` deve ser um de: `correct`, `incorrect`, `uncertain`.

**Response `200 OK`:** `{ "message": "Feedback recebido com sucesso" }`
**Erros:** `400` (emailId inválido ou feedback fora do enum).

---

### GET `/api/emails/by-classification/{classification}`

Lista e-mails filtrados por classificação, com paginação.

**Parâmetros de rota:** `classification` — um de `safe`, `suspicious`, `phishing`.

**Query string:**

| Parâmetro | Default | Descrição |
|-----------|---------|-----------|
| `skip` | 0 | Registros a pular (paginação) |
| `take` | 10 | Registros a retornar |

**Response `200 OK`:**
```json
[
  {
    "id": 42,
    "subject": "URGENTE: Verifique sua conta",
    "sender": "alert@bank-verify.tk",
    "classification": "phishing",
    "confidence": 92.4,
    "receivedDate": "2026-08-30T09:00:00Z",
    "processedDate": "2026-08-30T09:00:01Z",
    "isValidated": false,
    "userFeedback": null
  }
]
```

---

## 5. Health Check

### GET `/health`

Endpoint sem prefixo `/api`, usado por monitoramento/orquestração para verificar se
a API e a conexão com o banco estão saudáveis.

**Autenticação:** não requerida.

**Response `200 OK`:** `{ "status": "healthy", "timestamp": "2026-08-30T12:00:00Z" }`
**Response `503`:** `{ "status": "unhealthy", "error": "..." }`

---

## 6. Convenções Gerais

- Todas as datas são retornadas em **UTC**, formato ISO 8601.
- Todos os corpos de requisição/resposta são **JSON** (`Content-Type: application/json`).
- Nomes de propriedades JSON usam **camelCase** (ex.: `isValidated`, `receivedDate`).
- Paginação sempre via `skip`/`take` (offset-based), sem cursor.
- Não há versionamento de URL nesta versão (`/api/v1/...`); caso necessário no futuro,
  recomenda-se introduzir via cabeçalho `Api-Version` ou prefixo de rota, sem quebrar
  os endpoints atuais.
