# Definição do Modelo de Dados - Power BI

Este documento descreve o modelo de dados usado no dashboard Power BI do sistema de
detecção de phishing, incluindo tabelas de origem, relacionamentos, colunas calculadas
e medidas DAX. Ele serve como especificação para montar o `dashboard_template.pbix`
a partir dos artefatos em [queries/](queries) e [measures/](measures).

## 1. Fonte de Dados

Conector: **PostgreSQL** (`PostgreSQL.Database`), apontando para o mesmo banco usado
pelo backend (`database/schema.sql`). Modo de conectividade recomendado: **Import**
(com refresh agendado), já que o volume esperado (até ~10.000 e-mails/dia) é compatível
com importação diária/horária sem necessidade de DirectQuery.

## 2. Tabelas do Modelo

| Tabela Power BI | Origem (tabela/view) | Papel no modelo |
|------------------|----------------------|------------------|
| `Emails` | `emails` | Fato principal — um registro por e-mail classificado |
| `PhishingIndicators` | `phishing_indicators` | Fato secundário — indicadores que explicam a classificação |
| `UrlsDetected` | `urls_detected` | Fato secundário — URLs extraídas e sua reputação |
| `DailyStatistics` | `daily_statistics` | Fato agregado diário — usado para tendências e KPIs |
| `ModelPerformance` | `model_performance` | Fato de métricas do modelo ML ao longo do tempo |
| `Users` | `users` | Dimensão — departamento, papel, e para relatórios de auditoria |
| `SuspiciousSenders` | view `v_suspicious_senders` | Tabela pré-agregada para o relatório de remetentes suspeitos |
| `DateTable` | calculada via DAX (`CALENDAR`) | Dimensão de data para Time Intelligence |

## 3. Relacionamentos

```
Emails[id]  1 ── * PhishingIndicators[email_id]
Emails[id]  1 ── * UrlsDetected[email_id]
DateTable[Date] 1 ── * Emails[received_date] (relação por data, granularidade dia)
DateTable[Date] 1 ── * DailyStatistics[statistic_date]
DateTable[Date] 1 ── * ModelPerformance[tested_at] (granularidade dia)
```

- Todos os relacionamentos são **um-para-muitos**, direção única (Emails/DateTable → tabelas de fato).
- `Users` não se relaciona diretamente com `Emails` no schema atual (o backend não grava
  `reviewed_by` em `emails`); ela é usada isoladamente em relatórios de auditoria/consentimento LGPD.
  Se uma futura versão do backend adicionar rastreabilidade de quem validou o e-mail, um
  relacionamento `Users[id] 1 ── * Emails[reviewed_by]` pode ser adicionado.

## 4. Colunas Calculadas

| Tabela | Coluna | Fórmula (DAX) | Propósito |
|--------|--------|----------------|-----------|
| `Emails` | `ClassificationLabel` | `SWITCH(Emails[classification], "safe", "Seguro", "suspicious", "Suspeito", "phishing", "Phishing", Emails[classification])` | Rótulo em português para visuais |
| `Emails` | `ReceivedDateOnly` | `DATE(YEAR(Emails[received_date]), MONTH(Emails[received_date]), DAY(Emails[received_date]))` | Chave para relacionamento com `DateTable` |

## 5. Medidas DAX

Ver definições completas e comentadas em [measures/dax_measures.dax](measures/dax_measures.dax).
Resumo das medidas principais:

| Medida | Descrição |
|--------|-----------|
| `Total de E-mails` | Contagem total de e-mails processados |
| `Total Phishing` / `Total Suspeitos` / `Total Seguros` | Contagem por classificação |
| `Taxa de Phishing %` | % de e-mails classificados como phishing |
| `Confiança Média` | Média do score de confiança da classificação |
| `Taxa de Validação %` | % de e-mails já revisados por um analista |
| `Acurácia do Modelo (Atual)` | Última acurácia registrada em `ModelPerformance` |
| `Tempo Médio de Processamento (ms)` | Média de `avg_processing_time_ms` em `DailyStatistics` |
| `Variação de Phishing vs Período Anterior` | Time Intelligence — comparação com período anterior |

## 6. Páginas do Dashboard Sugeridas

1. **Visão Geral** — cartões de KPI (Total de E-mails, Taxa de Phishing %, Confiança Média,
   Acurácia do Modelo), gráfico de pizza por classificação, série temporal diária
   (`DailyStatistics`).
2. **Remetentes Suspeitos** — tabela `SuspiciousSenders` ordenada por `phishing_count`,
   com drill-through para os e-mails individuais desse remetente em `Emails`.
3. **Indicadores de Phishing** — gráfico de barras de `PhishingIndicators[indicator_type]`
   por frequência e `risk_score` médio; lista de URLs suspeitas (`UrlsDetected`).
4. **Desempenho do Modelo** — série temporal de `accuracy`, `precision`, `recall`, `f1_score`
   por `model_name` (Naive Bayes vs Random Forest vs Ensemble), comparando com as metas do
   projeto (>95%, >94%, >96%, >95%).
5. **Auditoria e LGPD** (opcional, acesso restrito) — contagem de `AuditLogs` por ação,
   status de `DataDeletionRequests`.

## 7. Atualização dos Dados (Refresh)

- **Local (Power BI Desktop)**: botão "Atualizar" ou agendamento via Power BI Gateway
  (necessário para on-premises, já que o requisito do projeto é não usar nuvem para os dados).
- **Frequência recomendada**: horária durante expediente, diária fora dele — configurável
  em `system_config.EMAIL_CHECK_INTERVAL_SECONDS` no backend não tem relação direta com o
  refresh do Power BI, que é configurado separadamente no Gateway/Power BI Service.
- **Gateway on-premises**: obrigatório, pois o requisito do projeto veda o uso de nuvem
  pública para armazenar os dados (LGPD + política interna do Health2B Hospitalar).
