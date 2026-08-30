# Frontend - Phishing Detection

Interface web em **React 18** para o sistema de detecção de phishing corporativo, consumindo a
API backend (.NET 10) descrita em [backend/README.md](../backend/README.md).

## 🏗️ Estrutura

```
frontend/
├── public/
│   └── index.html
├── src/
│   ├── components/          # Componentes reutilizáveis
│   │   ├── Navbar.js
│   │   ├── PrivateRoute.js
│   │   ├── ClassificationBadge.js
│   │   ├── StatCard.js
│   │   └── EmailTable.js
│   ├── context/
│   │   └── AuthContext.js   # Estado global de autenticação (JWT)
│   ├── pages/                # Telas da aplicação
│   │   ├── LoginPage.js
│   │   ├── DashboardPage.js
│   │   ├── EmailsPage.js
│   │   └── ClassifyEmailPage.js
│   ├── services/             # Camada de acesso à API (axios)
│   │   ├── api.js
│   │   ├── authService.js
│   │   └── emailService.js
│   ├── App.js
│   └── index.js
├── .env.example
└── package.json
```

## 📋 Tecnologias

| Componente | Versão | Descrição |
|-----------|--------|-----------|
| React | 18.2 | Biblioteca de UI |
| React Router | 6.22 | Roteamento SPA |
| Axios | 1.6 | Cliente HTTP |
| react-scripts | 5.0 | Build tooling (Create React App) |

## 🔐 Autenticação

O `AuthContext` mantém o usuário autenticado em memória e sincroniza com `localStorage`
(`token` e `user`). O `api.js` injeta automaticamente o header `Authorization: Bearer <token>`
em toda requisição e redireciona para `/login` caso o backend responda `401`.

## 🚀 Como Executar

```bash
cd frontend
npm install
cp .env.example .env      # ajuste REACT_APP_API_BASE_URL se necessário
npm start
```

A aplicação sobe em `http://localhost:3000` e espera o backend disponível em
`https://localhost:5001/api` (configurável via `REACT_APP_API_BASE_URL`).

## 📄 Páginas

| Rota | Descrição | Protegida |
|------|-----------|-----------|
| `/login` | Autenticação do usuário | ❌ |
| `/dashboard` | Estatísticas e e-mails de phishing recentes | ✅ |
| `/emails` | Lista de e-mails por classificação, com feedback | ✅ |
| `/classify` | Formulário para testar a classificação manualmente | ✅ |

## 🧪 Testes

```bash
npm test
```
