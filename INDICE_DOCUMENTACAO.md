# 📑 Índice de Documentação - Sistema de Detecção de Phishing

**Última atualização:** 30 de Agosto de 2026  
**Versão:** 1.0  

---

## 🎯 Comece Aqui

### Para Desenvolvedores
1. Ler: [README.md](README.md) - Instalação rápida
2. Ler: [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md) - Roadmap do projeto
3. Ler: [CONTRIBUINDO.md](CONTRIBUINDO.md) - Regras e padrões
4. Implementar: [Fase atual de desenvolvimento]

### Para Operações (TI/DevOps)
1. Ler: [README.md - Seção "Instalação no Servidor"](README.md#-instalação-no-servidor-da-empresa)
2. Ler: [docs/DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md) - Passo a passo
3. Preparar: Checklist de pré-requisitos
4. Executar: Deployment conforme guia

### Para Gerência/Stakeholders
1. Ler: [PLANO_DESENVOLVIMENTO.md - Visão Geral e Fases](PLANO_DESENVOLVIMENTO.md#1-visão-geral-do-projeto)
2. Ler: [PLANO_DESENVOLVIMENTO.md - Roadmap](PLANO_DESENVOLVIMENTO.md#5-fases-do-desenvolvimento)
3. Verificar: [Métricas de Sucesso](PLANO_DESENVOLVIMENTO.md#8-métricas-de-sucesso-machine-learning)

---

## 📚 Documentação Completa

### Configuração e Setup
| Documento | Descrição | Audiência |
|-----------|-----------|-----------|
| [README.md](README.md) | Visão geral, instalação e comandos básicos | Todos |
| [.env.example](.env.example) | Variáveis de ambiente necessárias | DevOps/Dev |
| [.gitignore](.gitignore) | Arquivos ignorados pelo Git | Desenvolvedores |

### Planejamento e Estratégia
| Documento | Descrição | Audiência |
|-----------|-----------|-----------|
| [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md) | Roadmap, arquitetura, tecnologias | Todos |
| [PLANO_DESENVOLVIMENTO.md #2](PLANO_DESENVOLVIMENTO.md#2-arquitetura-do-sistema) | Arquitetura detalhada | Arquitetos/Dev |
| [PLANO_DESENVOLVIMENTO.md #3](PLANO_DESENVOLVIMENTO.md#3-tecnologias-utilizadas) | Stack tecnológico | Todos |

### Desenvolvimento e Contribuição
| Documento | Descrição | Audiência |
|-----------|-----------|-----------|
| [CONTRIBUINDO.md](CONTRIBUINDO.md) | Regras, padrões de código, fluxo de PR | Desenvolvedores |
| [CONTRIBUINDO.md #6](CONTRIBUINDO.md#-padrões-de-código) | Convenções de nomenclatura | Desenvolvedores |
| [CONTRIBUINDO.md #8](CONTRIBUINDO.md#-processo-de-review) | Processo de review | Revisor de Código |

### Implementação em Produção
| Documento | Descrição | Audiência |
|-----------|-----------|-----------|
| [docs/DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md) | Instalação passo a passo no servidor | DevOps/SysAdmin |
| [docs/DEPLOYMENT_GUIDE.md #1](docs/DEPLOYMENT_GUIDE.md#-checklist-pré-deployment) | Checklist de pré-requisitos | Gerente/DevOps |
| [docs/DEPLOYMENT_GUIDE.md #2](docs/DEPLOYMENT_GUIDE.md#-passo-1-preparação-do-servidor-dia-1) | Preparação do servidor | DevOps |
| [docs/DEPLOYMENT_GUIDE.md #8](docs/DEPLOYMENT_GUIDE.md#-passo-8-testes-de-integração-dia-4) | Testes de integração | QA/DevOps |

### Banco de Dados
| Documento | Descrição | Audiência |
|-----------|-----------|-----------|
| [database/schema.sql](database/schema.sql) | Schema PostgreSQL completo | DBAs/Dev |

### Referência de Tecnologias
| Tecnologia | Documentação | Versão |
|-----------|-----------|--------|
| C# .NET | [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/) | 6+ |
| Python | [Python.org](https://docs.python.org/3/) | 3.9+ |
| React | [React Docs](https://react.dev) | 18+ |
| PostgreSQL | [PostgreSQL Docs](https://www.postgresql.org/docs/) | 13+ |
| scikit-learn | [scikit-learn Docs](https://scikit-learn.org/) | 1.0+ |
| Power BI | [MS Power BI Docs](https://learn.microsoft.com/en-us/power-bi/) | 2024+ |

---

## 🔍 Encontrar Informações por Tópico

### Instalação e Setup
- Instalação rápida: [README.md #-instalação-rápida](README.md#-instalação-rápida-windows-server)
- Instalação no servidor: [README.md #-instalação-no-servidor-da-empresa](README.md#-instalação-no-servidor-da-empresa)
- Deployment detalhado: [docs/DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md)
- Variáveis de ambiente: [.env.example](.env.example)

### Arquitetura e Design
- Visão geral: [PLANO_DESENVOLVIMENTO.md #2](PLANO_DESENVOLVIMENTO.md#2-arquitetura-do-sistema)
- Tecnologias: [PLANO_DESENVOLVIMENTO.md #3](PLANO_DESENVOLVIMENTO.md#3-tecnologias-utilizadas)
- Estrutura de pastas: [PLANO_DESENVOLVIMENTO.md #4](PLANO_DESENVOLVIMENTO.md#4-estrutura-de-pastas-do-projeto)

### Desenvolvimento
- Padrões de código: [CONTRIBUINDO.md #6](CONTRIBUINDO.md#-padrões-de-código)
- Fluxo de contribuição: [CONTRIBUINDO.md #5](CONTRIBUINDO.md#-fluxo-de-desenvolvimento-interno)
- Regras do projeto: [CONTRIBUINDO.md #-regras-do-projeto](CONTRIBUINDO.md#-regras-do-projeto-baseadas-no-artigo)

### Machine Learning
- Algoritmos: [PLANO_DESENVOLVIMENTO.md #4.3](database/schema.sql)
- Treinamento: [docs/DEPLOYMENT_GUIDE.md #4.2](docs/DEPLOYMENT_GUIDE.md#42-treinar-modelos-se-primeira-execução)
- Métricas: [PLANO_DESENVOLVIMENTO.md #8](PLANO_DESENVOLVIMENTO.md#8-métricas-de-sucesso-machine-learning)

### Segurança e LGPD
- Segurança: [PLANO_DESENVOLVIMENTO.md #10](PLANO_DESENVOLVIMENTO.md#10-considerações-de-segurança-lgpd)
- Firewall: [docs/DEPLOYMENT_GUIDE.md #6.2](docs/DEPLOYMENT_GUIDE.md#62-configurar-firewall)
- SSL/TLS: [docs/DEPLOYMENT_GUIDE.md #6.1](docs/DEPLOYMENT_GUIDE.md#61-instalar-certificado-ssltls)

### Troubleshooting
- Problemas comuns: [README.md #-troubleshooting](README.md#-troubleshooting)
- Monitoramento: [README.md #-monitoramento-e-manutenção](README.md#-monitoramento-e-manutenção)
- Rollback: [docs/DEPLOYMENT_GUIDE.md #9.3](docs/DEPLOYMENT_GUIDE.md#93-plano-de-rollback)

---

## 🚀 Fluxos de Trabalho Rápidos

### Iniciar Desenvolvimento Local
```
1. Ler: README.md (Pré-requisitos e Instalação Rápida)
2. Clonar: git clone [repositório]
3. Configurar: Copiar .env.example para .env
4. Instalar: Seguir etapas do README
5. Contribuir: Ler CONTRIBUINDO.md
```

### Deploy em Produção
```
1. Ler: docs/DEPLOYMENT_GUIDE.md (integralmente)
2. Preparar: Checklist de pré-requisitos
3. Executar: Passos 1-7 do guia
4. Testar: Passo 8 (Testes de Integração)
5. Go-Live: Passo 9 (Go-Live)
```

### Adicionar Funcionalidade
```
1. Ler: PLANO_DESENVOLVIMENTO.md (requisitos)
2. Ler: CONTRIBUINDO.md (padrões)
3. Criar: feature/descricao-funcionalidade
4. Desenvolver: Seguir padrões
5. Testar: 80%+ cobertura
6. PR: Descrever mudanças
7. Review: Aguardar aprovação
```

---

## 📋 Checklist de Leitura (por Perfil)

### Novo Desenvolvedor
- [ ] README.md (completo)
- [ ] PLANO_DESENVOLVIMENTO.md (Visão Geral + Arquitetura)
- [ ] CONTRIBUINDO.md (completo)
- [ ] Padrões de código (seu módulo)

### DevOps/SysAdmin
- [ ] README.md (Instalação no Servidor)
- [ ] docs/DEPLOYMENT_GUIDE.md (completo)
- [ ] .env.example
- [ ] database/schema.sql

### Product Manager/Gerente
- [ ] PLANO_DESENVOLVIMENTO.md (Visão Geral + Fases + Métricas)
- [ ] README.md (resumo)
- [ ] CONTRIBUINDO.md (Regras do Projeto)

### QA/Tester
- [ ] README.md (Troubleshooting)
- [ ] PLANO_DESENVOLVIMENTO.md (Requisitos Funcionais)
- [ ] docs/DEPLOYMENT_GUIDE.md (Testes)
- [ ] Casos de teste

### Pesquisador/Acadêmico
- [ ] PLANO_DESENVOLVIMENTO.md (completo)
- [ ] docs/DEPLOYMENT_GUIDE.md (setup)
- [ ] database/schema.sql
- [ ] Documentação de ML

---

## 🔗 Links Rápidos

### Documentação Interna
- 📘 [Plano de Desenvolvimento](PLANO_DESENVOLVIMENTO.md)
- 📖 [README Principal](README.md)
- 📝 [Guia de Contribuição](CONTRIBUINDO.md)
- 🚀 [Guia de Deployment](docs/DEPLOYMENT_GUIDE.md)
- 💾 [Schema do Banco](database/schema.sql)
- ⚙️ [Variáveis de Ambiente](.env.example)

### Referências Externas
- 🔵 [C# .NET Docs](https://docs.microsoft.com/en-us/dotnet/)
- 🐍 [Python Docs](https://docs.python.org/3/)
- ⚛️ [React Docs](https://react.dev)
- 🐘 [PostgreSQL Docs](https://www.postgresql.org/docs/)
- 🤖 [scikit-learn Docs](https://scikit-learn.org/)
- 📊 [Power BI Docs](https://learn.microsoft.com/en-us/power-bi/)

### Artigo Original
- 📄 [TCC_1.pdf](TCC_1.pdf) - Artigo completo

---

## ❓ Perguntas Frequentes

**P: Por onde começo?**  
R: Depende do seu perfil:
- Desenvolvedores: [README.md](README.md) → [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md) → [CONTRIBUINDO.md](CONTRIBUINDO.md)
- DevOps: [README.md (seção servidor)](README.md#-instalação-no-servidor-da-empresa) → [docs/DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md)
- Gerência: [PLANO_DESENVOLVIMENTO.md](PLANO_DESENVOLVIMENTO.md)

**P: Onde está a arquitetura?**  
R: [PLANO_DESENVOLVIMENTO.md #2](PLANO_DESENVOLVIMENTO.md#2-arquitetura-do-sistema)

**P: Como faço deploy?**  
R: [docs/DEPLOYMENT_GUIDE.md](docs/DEPLOYMENT_GUIDE.md) (guia passo a passo)

**P: Qual é a stack tecnológica?**  
R: [PLANO_DESENVOLVIMENTO.md #3](PLANO_DESENVOLVIMENTO.md#3-tecnologias-utilizadas)

**P: Como contribuir?**  
R: [CONTRIBUINDO.md](CONTRIBUINDO.md) (regras e fluxo)

**P: Quais são as métricas de sucesso?**  
R: [PLANO_DESENVOLVIMENTO.md #8](PLANO_DESENVOLVIMENTO.md#8-métricas-de-sucesso-machine-learning)

---

## 📞 Suporte

Para questões sobre documentação:
1. Verificar este índice
2. Buscar no documento relevante
3. Consultar [CONTRIBUINDO.md](CONTRIBUINDO.md) - Seção Contato
4. Abrir issue no repositório

---

**Versão:** 1.0  
**Data:** 30 de Agosto de 2026  
**Status:** Documentação Completa
