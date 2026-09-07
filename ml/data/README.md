# Datasets de Treinamento

Esta pasta guarda os datasets usados para treinar os modelos de classificação.
Os dados brutos **não são versionados** (ver `.gitignore`): `data/raw/` e `data/dataset.csv`
ficam fora do Git por tamanho e licenciamento. Apenas `data/dataset_stats.json` (metadados
da consolidação) é versionado.

## Formato canônico

`training.build_dataset` produz `data/dataset.csv` com estas colunas — o mesmo formato que
`train_model.py` / `evaluate_model.py` esperam:

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `subject` | string | Assunto do e-mail |
| `body` | string | Corpo do e-mail (texto ou HTML) |
| `sender` | string | Endereço do remetente |
| `label` | string | Uma de: `safe`, `suspicious`, `phishing` |

```csv
subject,body,sender,label
"Reunião de equipe amanhã","Olá, confirmando nossa reunião às 10h.",joao@empresa.com,safe
"URGENTE: verifique sua conta","Clique aqui: http://192.168.1.1/login",security@bank-alert.tk,phishing
```

## Fontes de dados

O pacote `training/datasets/` traz um loader por fonte; todos normalizam para o formato
canônico e mapeiam os rótulos de origem para `safe`/`suspicious`/`phishing`
(ver `training/datasets/label_mapping.py`).

### 1. Sting9 Research Initiative (fonte primária)

API: **`https://api.sting9.org/api/v1`** (host `api.sting9.org`, não `sting9.org`).

Categorias nativas são todas maliciosas → viram `phishing` (engano direto: `smishing`,
`account_alert`, `delivery`, `finance_crypto`, `prize_contest`, `phishing`) ou `suspicious`
(`spam`, `advertisement`, `job_advertisement`, `loans_credit`, `tech_support_scam`,
`wrong_number_romance_scam`, ...); `other` é descartado. Mapa em
`training/datasets/label_mapping.py` — **revisar após o 1º treino**.
**Não fornece exemplos `safe`.** Licença **ODC-BY-NC** (não-comercial, exige atribuição):
cite a fonte no TCC e **não** versione o JSON bruto.

> ⚠️ O acervo do Sting9 é **majoritariamente SMS/smishing** (~2260 SMS vs ~68 e-mails no
> total — ver `GET /api/v1/stats`). Para um classificador de **e-mail corporativo** ele
> contribui pouco como e-mail; sirva-se dele como sinal de "texto malicioso/golpe" e
> registre isso como limitação (metodologia 2.7), ou filtre `--type email` (poucas linhas)
> e complemente `suspicious` com outra base.

```bash
# 1. Registre-se:  POST https://api.sting9.org/api/v1/auth/register  (e-mail, senha, organização)
# 2. Login:        POST https://api.sting9.org/api/v1/auth/login  -> copie o JWT
export STING9_TOKEN="<jwt>"
# 3. Baixe o export (snapshot reprodutível em data/raw/):
python -m training.fetch_sting9 --out data/raw/sting9_export_$(date +%F).json
```

`fetch_sting9` pagina `GET /api/v1/export?format=json` (page_size 1000) e, se o endpoint
não existir, cai para `GET /api/v1/submissions`. Grava o JSON + um `.meta.json`
(endpoint, contagem, data).

### 2. Phishing Email Dataset (Kaggle)

Dataset binário (`safe` vs `phishing`) — é a **única fonte de exemplos `safe`**.
Baixe manualmente e aponte o CSV, ou configure as credenciais Kaggle para baixar via CLI:

```bash
# opção A: CSV já baixado
python -m training.build_dataset --sources kaggle_phishing --kaggle-csv /caminho/phishing_email.csv

# opção B: download automático
export KAGGLE_USERNAME=... KAGGLE_KEY=...
export KAGGLE_DATASET_SLUG=owner/phishing-email-dataset
python -m training.build_dataset --sources kaggle_phishing
```

As colunas de texto/rótulo são autodetectadas (`Email Text`/`text`/`body`,
`Email Type`/`label`/`0-1`).

### 3. CSV de spam como classe intermediária (`spam_csv`)

Mesma leitura do `kaggle_phishing`, mas o rótulo malicioso (`1`) é mapeado para
**`suspicious`** em vez de `phishing`. Serve para transformar um corpus binário de spam
(ex.: CEAS 2008) na classe intermediária quando não há uma fonte dedicada de `suspicious`
(o Sting9 seria a fonte natural).

```bash
# CEAS 2008 (1 -> suspicious) + um dataset de phishing (1 -> phishing):
python -m training.build_dataset --sources kaggle_phishing,spam_csv \
  --kaggle-csv /caminho/phishing_email_dataset.csv \
  --spam-csv   /caminho/ceas_08.csv \
  --out data/dataset.csv
```

Combinação de exemplo já validada: `safe` (rótulo `0` das duas fontes) · `phishing`
(rótulo `1` do dataset de phishing) · `suspicious` (rótulo `1` do CEAS).

### 4. CSV local (fallback)

Um CSV já no formato canônico (curado à mão). É o default do `build_dataset` e dos
scripts de treino, para o pipeline rodar sem acesso a Sting9/Kaggle.

## Consolidar as fontes

```bash
python -m training.build_dataset --sources sting9,kaggle_phishing --out data/dataset.csv
```

Concatena as fontes, **remove duplicatas** (exatas e variações só de espaço no corpo),
loga a distribuição de classes por fonte e no total, e grava:

- `data/dataset.csv` — dataset consolidado
- `data/dataset_stats.json` — contagens, fontes, duplicatas removidas, `sha256`, timestamp
  (versionado, entra como anexo de reprodutibilidade do TCC)

## Dataset sintético (somente smoke test)

```bash
python -m training.generate_sample_dataset
```

Cria `data/dataset.csv` com ~15 exemplos artificiais. Serve só para exercitar o pipeline;
`train_model.py` **aborta** com esse volume (mínimo de 50 por classe). **Não use para métricas.**
