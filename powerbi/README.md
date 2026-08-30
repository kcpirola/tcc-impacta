# Power BI - Dashboard de Detecção de Phishing

Este diretório contém os artefatos usados para montar o `dashboard_template.pbix`
mencionado no plano de desenvolvimento. Como o `.pbix` é um formato binário, ele **não é
gerado por ferramentas de texto/código** — em vez disso, este diretório traz tudo que é
necessário para reconstruí-lo no Power BI Desktop em poucos passos.

## Conteúdo

```
powerbi/
├── README.md                      # Este arquivo
├── data_model_definition.md       # Tabelas, relacionamentos e medidas (especificação)
├── queries/                       # Scripts Power Query M (Editor Avançado)
│   ├── Emails.pq
│   ├── PhishingIndicators.pq
│   ├── UrlsDetected.pq
│   ├── DailyStatistics.pq
│   ├── ModelPerformance.pq
│   ├── Users.pq
│   └── SuspiciousSenders.pq
└── measures/                      # Medidas DAX prontas para colar
    ├── dax_measures.dax
    └── date_table.dax
```

## Pré-requisitos

- Power BI Desktop instalado (Windows).
- Driver **Npgsql** para PostgreSQL instalado (necessário para o conector nativo do
  Power BI a PostgreSQL). Baixe em: https://www.postgresql.org/ftp/odbc/versions/msi/
- Banco de dados já criado e populado (`database/schema.sql` + `database/seed.sql`,
  ver [scripts/setup_database.ps1](../scripts/setup_database.ps1)).
- Acesso de rede ao servidor PostgreSQL a partir da máquina com o Power BI Desktop.

## Passo a passo para montar o `.pbix`

1. Abra o Power BI Desktop → **Página inicial > Obter dados > Banco de dados PostgreSQL**.
2. Informe o servidor/porta e banco (`localhost`, `phishing_detection` ou os valores reais
   de produção) e escolha o modo **Import**.
3. Na janela do Navegador, clique em **Transformar Dados** para abrir o Power Query Editor.
4. Para cada arquivo em [queries/](queries):
   - Crie uma **Nova Consulta em Branco**.
   - Abra o **Editor Avançado** e cole o conteúdo do arquivo `.pq` correspondente.
   - Renomeie a consulta com o nome do arquivo (ex.: `Emails`, `PhishingIndicators`).
   - Ajuste `Server`/`Database` na primeira linha (`PostgreSQL.Database("localhost", "phishing_detection")`)
     para apontar ao seu ambiente, se necessário.
5. Clique em **Fechar e Aplicar** para carregar as tabelas no modelo.
6. Em **Modelagem > Nova Tabela**, cole o conteúdo de [measures/date_table.dax](measures/date_table.dax)
   para criar a dimensão de datas. Marque-a como **Tabela de Datas** (coluna `Date`).
7. Configure os relacionamentos descritos em
   [data_model_definition.md](data_model_definition.md#3-relacionamentos):
   - `Emails[id]` → `PhishingIndicators[email_id]` (um-para-muitos)
   - `Emails[id]` → `UrlsDetected[email_id]` (um-para-muitos)
   - `DateTable[Date]` → `Emails[ReceivedDateOnly]`
   - `DateTable[Date]` → `DailyStatistics[statistic_date]`
   - `DateTable[Date]` → `ModelPerformance[TestedDateOnly]`
8. Crie uma tabela de medidas dedicada (opcional, mas recomendado): **Modelagem > Nova Tabela**
   com `_Medidas = {BLANK()}`, depois cole cada medida de
   [measures/dax_measures.dax](measures/dax_measures.dax) individualmente (**Nova Medida**).
9. Monte as páginas do relatório seguindo a sugestão em
   [data_model_definition.md](data_model_definition.md#6-páginas-do-dashboard-sugeridas)
   (Visão Geral, Remetentes Suspeitos, Indicadores de Phishing, Desempenho do Modelo, Auditoria/LGPD).
10. Salve como `powerbi/dashboard_template.pbix`.

## Publicação e Atualização Automática (on-premises)

Como o projeto veda o uso de nuvem pública para os dados (requisito LGPD/Health2B):

1. Publique o relatório em um **Power BI Report Server on-premises** (não o serviço
   Power BI na nuvem da Microsoft), ou distribua o `.pbix` internamente.
2. Instale o **On-premises Data Gateway** na mesma rede do PostgreSQL para permitir
   atualização agendada sem expor o banco à internet.
3. Configure o agendamento de atualização (recomendado: horário em expediente, diário
   fora dele) apontando para o gateway.
4. Restrinja o acesso ao relatório por grupo (RBAC do Power BI Report Server), alinhado
   aos papéis já existentes no backend (`User`, `Analyst`, `Admin`).

## Segurança e LGPD

- A consulta [Users.pq](queries/Users.pq) **remove explicitamente** a coluna `password_hash`
  antes de carregar os dados — nunca reverta essa remoção.
- Não inclua `email_body`/`email_headers` no modelo (já removidos em [Emails.pq](queries/Emails.pq))
  para minimizar exposição de dados pessoais/sensíveis no relatório.
- Aplique Row-Level Security (RLS) no Power BI se o relatório for compartilhado entre
  departamentos que não devem ver dados uns dos outros.
