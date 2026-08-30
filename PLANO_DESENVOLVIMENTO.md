# Plano de Desenvolvimento - Sistema de Detecção de Phishing

## 1. Visão Geral do Projeto

Sistema automatizado de detecção e classificação de e-mails de phishing em ambiente corporativo, desenvolvido para a Health2B Hospitalar Ltda. O sistema analisa características específicas de e-mails e os classifica em três categorias: **seguro**, **suspeito** ou **phishing**.

### Objetivos
- Detectar e-mails maliciosos de forma automatizada
- Reduzir dependência exclusiva da análise humana
- Gerar alertas em tempo real
- Produzir relatórios e dashboards interativos
- Avaliar desempenho técnico e percepção dos usuários

---

## 2. Arquitetura do Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND (React)                           │
│                  Interface do Usuário                           │
│              Alertas em Tempo Real                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  BACKEND (C#/.NET)                              │
│    • Integração com servidor de e-mail                          │
│    • Orquestração de processos                                  │
│    • Autenticação e autorização                                 │
│    • API REST para comunicação                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│          MICROSERVIÇO ML (Python)                               │
│    • Classificação via Naive Bayes                              │
│    • Classificação via Random Forest                            │
│    • Processamento de Linguagem Natural (TF-IDF)                │
│    • Análise de características de e-mail                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                 RELATÓRIOS (Power BI)                           │
│           Dashboard Interativo de Análise                       │
│              Visualização de Padrões                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Tecnologias Utilizadas

| Tecnologia | Versão | Finalidade | Camada |
|-----------|--------|-----------|--------|
| C# | .NET 10 | Integração email, orquestração | Backend |
| Python | 3.9+ | Classificação e ML | Machine Learning |
| TF-IDF | scikit-learn | Vetorização de texto | ML |
| Naive Bayes | scikit-learn | Triagem inicial | ML |
| Random Forest | scikit-learn | Classificação final | ML |
| React | 18+ | Interface do usuário | Frontend |
| Power BI | 2024+ | Dashboard e relatórios | Relatórios |
| PostgreSQL | 13+ | Banco de dados | Database |
| Sting9 API | v1.0 | Dataset de treinamento | Data Source |
| Kaggle API | v1.0 | Dataset de treinamento | Data Source |

---

## 4. Estrutura de Pastas do Projeto

```
tcc-impacta/
├── backend/                          # Backend C# .NET
│   ├── PhishingDetectionAPI/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Data/
│   │   └── appsettings.json
│   └── PhishingDetectionAPI.csproj
│
├── ml/                               # Microserviço Python ML
│   ├── models/                       # Modelos treinados
│   │   ├── naive_bayes_model.pkl
│   │   └── random_forest_model.pkl
│   ├── training/                     # Scripts de treinamento
│   │   ├── train_model.py
│   │   ├── evaluate_model.py
│   │   └── data_preprocessing.py
│   ├── service/                      # API Flask
│   │   ├── classification_service.py
│   │   └── app.py
│   ├── data/                         # Datasets
│   │   ├── sting9/
│   │   ├── kaggle/
│   │   └── README.md
│   ├── requirements.txt
│   └── config.py
│
├── frontend/                         # Interface React
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   ├── services/
│   │   ├── App.js
│   │   └── index.js
│   ├── public/
│   ├── package.json
│   └── .env.example
│
├── database/                         # Scripts SQL
│   ├── schema.sql
│   ├── migrations/
│   └── seed.sql
│
├── powerbi/                          # Configurações Power BI
│   ├── dashboard_template.pbix
│   └── data_model_definition.md
│
├── docs/                             # Documentação
│   ├── ARQUITETURA.md
│   ├── API_SPEC.md
│   ├── USUARIO_GUIDE.md
│   └── DEPLOYMENT_GUIDE.md
│
├── tests/                            # Testes
│   ├── unit/
│   ├── integration/
│   └── performance/
│
├── scripts/                          # Scripts auxiliares
│   ├── install_dependencies.sh
│   ├── setup_database.sh
│   ├── train_models.sh
│   └── start_services.sh
│
├── config/                           # Arquivos de configuração
│   ├── docker-compose.yml
│   ├── .env.example
│   └── nginx.conf
│
├── PLANO_DESENVOLVIMENTO.md          # Este arquivo
├── README.md                         # Instruções de instalação
├── CONTRIBUINDO.md                   # Guia de contribuição
└── .gitignore
```

---

## 5. Fases do Desenvolvimento

### Fase 1: Preparação do Ambiente (Semana 1)
- [x] Inicializar repositório Git
- [ ] Configurar ambiente de desenvolvimento local
- [ ] Criar estrutura de pastas
- [ ] Configurar banco de dados PostgreSQL
- [ ] Documentar requisitos e dependências

**Deliverables:**
- Estrutura de projeto criada
- Banco de dados configurado
- Variáveis de ambiente definidas

---

### Fase 2: Backend (Semana 2-3)
- [ ] Criar projeto C# .NET 6+
- [ ] Implementar autenticação e autorização
- [ ] Desenvolver controllers REST
- [ ] Integrar com servidor de e-mail (IMAP/SMTP)
- [ ] Criar camada de dados (Entity Framework)
- [ ] Implementar logging e tratamento de erros
- [ ] Desenvolver serviço de fila (para processamento assíncrono)

**Deliverables:**
- API REST funcional
- Integração com email server
- Testes unitários (80% de cobertura)

---

### Fase 3: Machine Learning (Semana 3-4)
- [ ] Baixar e preparar datasets (Sting9 + Kaggle)
- [ ] Implementar data preprocessing
- [ ] Extrair features com TF-IDF
- [ ] Treinar modelo Naive Bayes
- [ ] Treinar modelo Random Forest
- [ ] Validação cruzada (k-fold com k=10)
- [ ] Avaliar métricas (acurácia, precisão, recall, F1-score)
- [ ] Serializar e salvar modelos treinados
- [ ] Criar Flask API para inferência

**Deliverables:**
- Modelos treinados (.pkl)
- Relatório de desempenho
- Microserviço ML operacional

---

### Fase 4: Frontend (Semana 4-5)
- [ ] Criar projeto React
- [ ] Desenvolver componentes de UI
- [ ] Implementar dashboard de alertas
- [ ] Criar integração com API Backend
- [ ] Adicionar visualizações em tempo real
- [ ] Responsividade e acessibilidade
- [ ] Testes de componentes

**Deliverables:**
- Interface funcional
- Alertas em tempo real
- Dashboard de usuário

---

### Fase 5: Integração e Testes (Semana 5-6)
- [ ] Testes de integração entre componentes
- [ ] Testes de performance
- [ ] Testes de segurança (OWASP Top 10)
- [ ] Validação com dados reais (simulados)
- [ ] Documentação final
- [ ] Plano de deployment

**Deliverables:**
- Testes de aceitação aprovados
- Documentação de deployment
- Manual do usuário

---

### Fase 6: Power BI e Relatórios (Semana 6-7)
- [ ] Modelar dados para Power BI
- [ ] Criar dashboards interativos
- [ ] Configurar refresh automático
- [ ] Desenvolver relatórios de segurança
- [ ] Criar alertas e notificações

**Deliverables:**
- Dashboard Power BI operacional
- Relatórios de monitoramento

---

### Fase 7: Deployment e Produção (Semana 7-8)
- [ ] Preparar servidor de produção
- [ ] Configurar CI/CD
- [ ] Realizar deployment
- [ ] Monitoramento e logging em produção
- [ ] Suporte pós-implementação

**Deliverables:**
- Sistema em produção
- Documentação de operações
- Plano de manutenção

---

## 6. Requisitos Funcionais (Baseado na Pesquisa Qualitativa)

### RF-001: Detecção Automática
O sistema deve receber e-mails automaticamente via API de integração com servidor.

### RF-002: Classificação Automática
Cada e-mail deve ser classificado em: **seguro**, **suspeito** ou **phishing**.

### RF-003: Alertas em Tempo Real
Notificar usuário instantaneamente quando e-mail suspeito/phishing for detectado.

### RF-004: Rastreabilidade
Manter histórico completo de e-mails com motivo da classificação.

### RF-005: Relatórios
Gerar relatórios sobre frequência, padrões e tipos de ataques detectados.

### RF-006: Dashboard Interativo
Prover dashboard Power BI com visualizações de dados.

### RF-007: Aplicação de Labels
Adicionar labels/tags aos e-mails classificados para fácil identificação.

### RF-008: Feedback do Usuário
Permitir que usuários retroalimentem o sistema com classificações corretas.

---

## 7. Requisitos Não-Funcionais

| Requisito | Descrição |
|-----------|-----------|
| **Performance** | Classificação em < 2 segundos por e-mail |
| **Disponibilidade** | 99.5% de uptime |
| **Segurança** | Conformidade com LGPD e OWASP Top 10 |
| **Escalabilidade** | Suportar 10.000+ e-mails/dia |
| **Usabilidade** | Interface intuitiva (SUS > 70) |
| **Manutenibilidade** | Código limpo com 80%+ de cobertura de testes |

---

## 8. Métricas de Sucesso (Machine Learning)

O modelo deve atingir os seguintes benchmarks:

| Métrica | Meta |
|---------|------|
| **Acurácia** | > 95% |
| **Precisão** | > 94% |
| **Recall** | > 96% |
| **F1-Score** | > 95% |

**Obs:** Baseado em Naive Bayes + Random Forest ensemble

---

## 9. Dependências Externas

### Datasets
- **Sting9 Research Initiative API**: E-mails de phishing reais
- **Kaggle Phishing Dataset**: Dataset público de e-mails rotulados

### Serviços
- **Servidor IMAP**: Acesso aos e-mails
- **PostgreSQL**: Banco de dados
- **Power BI**: Dashboard e relatórios

### Bibliotecas Python
```
scikit-learn>=1.0.0
pandas>=1.3.0
numpy>=1.20.0
flask>=2.0.0
flask-cors>=3.0.0
requests>=2.26.0
python-dotenv>=0.19.0
```

### NuGet Packages (.NET)
```
EntityFrameworkCore
MailKit
Newtonsoft.Json
Serilog
FluentValidation
Xunit
```

---

## 10. Considerações de Segurança (LGPD)

1. **Privacidade de Dados**
   - Anonymizar e-mails no armazenamento
   - Criptografar dados sensíveis
   - Implementar RBAC (Role-Based Access Control)

2. **Conformidade LGPD**
   - Obter consentimento dos usuários
   - Direito ao esquecimento (delete de dados)
   - Auditoria de acessos
   - Relatórios de processamento

3. **Segurança da Informação (Tríade CIA)**
   - **Confidencialidade**: Criptografia TLS/SSL
   - **Integridade**: Hash e assinatura digital
   - **Disponibilidade**: Redundância e backup

---

## 11. Plano de Testes

### Testes Unitários
- Testar componentes isolados
- Coverage mínimo: 80%
- Framework: xUnit (Backend), Jest (Frontend)

### Testes de Integração
- Testar fluxos entre componentes
- Validar API REST
- Testar integração com BD

### Testes de Performance
- Benchmark de classificação
- Teste de carga (10.000+ e-mails)
- Análise de recursos

### Testes de Segurança
- SQL Injection
- XSS (Cross-Site Scripting)
- CSRF Protection
- Autenticação e autorização

---

## 12. Roadmap Geral

```
Semana 1: Preparação
   └─ Ambiente + Estrutura

Semana 2-3: Backend
   └─ API + Email Server

Semana 3-4: ML
   └─ Modelos + Treinamento

Semana 4-5: Frontend
   └─ UI + Dashboard

Semana 5-6: Integração + Testes
   └─ Sistema completo

Semana 6-7: Power BI
   └─ Relatórios

Semana 7-8: Production
   └─ Deployment + Operações
```

---

## 13. Próximos Passos

1. ✅ Revisar este plano com stakeholders
2. ✅ Confirmar recursos e timeline
3. ✅ Iniciar Fase 1: Preparação do Ambiente
4. ✅ Criar repositório Git com .gitignore apropriado
5. ✅ Configurar ferramentas de CI/CD
6. ✅ Estabelecer padrões de código e documentação

---

**Documento versão:** 1.0  
**Data de criação:** 30 de Agosto de 2026  
**Última atualização:** 30 de Agosto de 2026
