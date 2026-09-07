# Microserviço de Machine Learning - Detecção de Phishing

Serviço Python responsável por classificar e-mails em **safe**, **suspicious** ou **phishing**,
usando um ensemble ponderado de **Naive Bayes** + **Random Forest** (decisão por argmax da
probabilidade combinada), sobre features de **TF-IDF** somadas a indicadores heurísticos estruturais.

## 🏗️ Estrutura

```
ml/
├── config.py                          # Configurações centralizadas (paths, pesos do ensemble, treino, fontes)
├── requirements.txt                   # Dependências Python
├── training/
│   ├── data_preprocessing.py         # Limpeza, normalização e remoção de stopwords (SRP)
│   ├── feature_extraction.py         # Indicadores heurísticos (URLs, urgência, etc.)
│   ├── datasets/                     # Loaders plugáveis (sting9, kaggle_phishing, local_csv)
│   ├── fetch_sting9.py              # Baixa o export Sting9 via API (passo manual)
│   ├── build_dataset.py            # Consolida as fontes -> data/dataset.csv + stats
│   ├── train_model.py                # Treinamento com validação cruzada k-fold
│   ├── evaluate_model.py             # Avaliação de métricas vs. metas do projeto
│   └── generate_sample_dataset.py    # Dataset sintético para smoke test
├── service/
│   ├── classification_service.py     # Lógica de inferência do ensemble
│   └── app.py                        # API Flask consumida pelo backend .NET
├── models/                            # Modelos treinados (.pkl, não versionados) + metrics.json
└── data/                              # Datasets (dataset.csv/raw não versionados; dataset_stats.json sim)
```

## 🧠 Pipeline de Classificação

```
Texto bruto (subject + body)
   ↓
EmailPreprocessor.clean()          → remove HTML/URLs/e-mails, normaliza, remove stopwords PT+EN
   ↓
TfidfVectorizer.transform()        → vetor esparso de até 5000 features (uni+bigramas)
   ↓
PhishingFeatureExtractor.extract() → 12 features heurísticas (URLs suspeitas, urgência, etc.)
   ↓
MinMaxScaler.transform()           → normaliza features heurísticas para [0,1]
   ↓
hstack([tfidf, heurísticas])       → vetor combinado
   ↓
NaiveBayes.predict_proba()  ⟍
                              ⟩ Ensemble ponderado (35% NB + 65% RF)
RandomForest.predict_proba() ⟋
   ↓
argmax da probabilidade ponderada  → classe final (safe / suspicious / phishing)
   ↓
ClassificationResponse (classification, confidence, indicators, suspiciousUrls, recommendation)
```

## 📊 Features Heurísticas Extraídas

| Feature | Descrição |
|---------|-----------|
| `num_urls` / `num_suspicious_urls` | Quantidade de URLs e URLs suspeitas |
| `has_ip_url` | URL aponta diretamente para um IP |
| `has_shortened_url` | Uso de encurtadores (bit.ly, tinyurl, etc.) |
| `num_urgent_keywords` | Termos de urgência ("urgente", "clique aqui", etc.) |
| `subject_has_urgent_word` | Urgência presente no assunto |
| `num_exclamation_marks` | Quantidade de "!" no corpo |
| `num_uppercase_words` | Palavras totalmente em maiúsculas |
| `sender_domain_mismatch` | Domínio do remetente diferente das URLs do corpo |
| `body_length` | Tamanho do corpo do e-mail |
| `num_attachments_mentioned` | Menções a anexos |
| `has_generic_greeting` | Saudação genérica ("Dear Customer", "Caro cliente") |

## 🚀 Como Usar

### 1. Instalar dependências

```bash
cd ml
python -m venv venv
venv\Scripts\activate          # Windows
pip install -r requirements.txt
```

### 2. Consolidar o dataset

```bash
# smoke test (sem dados reais; o treino vai abortar por poucos dados):
python -m training.generate_sample_dataset

# dados reais — ver data/README.md para obter Sting9 e Kaggle:
python -m training.build_dataset --sources sting9,kaggle_phishing --out data/dataset.csv
```

`build_dataset` concatena as fontes, remove duplicatas e grava `data/dataset.csv` +
`data/dataset_stats.json`.

### 3. Treinar os modelos

```bash
python -m training.train_model --dataset data/dataset.csv
```

Gera em `models/`: `naive_bayes_model.pkl`, `random_forest_model.pkl`, `tfidf_vectorizer.pkl`,
`feature_scaler.pkl`, `label_encoder.pkl` e `cv_metrics.json`. Aborta se alguma classe tiver
menos de `ML_MIN_SAMPLES_PER_CLASS` (default 50) amostras.

### 4. Avaliar os modelos

```bash
python -m training.evaluate_model --dataset data/dataset.csv
```

Compara acurácia, precisão, recall e F1-score (NB, RF e ensemble) com as metas do projeto
(>95%, >94%, >96%, >95%) e grava `models/metrics.json` + `models/METRICS.md`.

### 5. Executar o microserviço

```bash
python -m service.app
# ou em produção:
gunicorn --chdir service --bind 0.0.0.0:5000 app:app
```

O serviço sobe em `http://localhost:5000`, o mesmo endereço configurado em
`backend/PhishingDetectionAPI/appsettings.json` → `MlServiceSettings.BaseUrl`.

## 📡 Endpoints

### `GET /health`
Verifica se os modelos estão carregados.

```json
{ "status": "healthy", "service": "phishing-ml-service" }
```

### `POST /classify`
Classifica um único e-mail.

**Request:**
```json
{
  "subject": "URGENTE: Verifique sua conta",
  "body": "Clique aqui: http://192.168.1.1/login",
  "sender": "alert@bank-verify.tk",
  "recipients": "user@empresa.com"
}
```

**Response:**
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

### `POST /classify-batch`
Recebe uma lista de e-mails e retorna uma lista de classificações no mesmo formato.

### `POST /reload-models`
Recarrega os modelos do disco após um novo treinamento, sem reiniciar o serviço.

## ⚙️ Configuração (variáveis de ambiente)

| Variável | Default | Descrição |
|----------|---------|-----------|
| `ML_SERVICE_HOST` | `0.0.0.0` | Host do Flask |
| `ML_SERVICE_PORT` | `5000` | Porta do Flask |
| `NAIVE_BAYES_WEIGHT` | `0.35` | Peso do NB no ensemble |
| `RANDOM_FOREST_WEIGHT` | `0.65` | Peso do RF no ensemble |
| `ML_CV_FOLDS` | `10` | Número de folds na validação cruzada |
| `ML_MIN_SAMPLES_PER_CLASS` | `50` | Mínimo de amostras por classe para permitir o treino |
| `TFIDF_MAX_FEATURES` | `5000` | Máximo de features do TF-IDF |
| `STING9_API_BASE` | `https://api.sting9.org/api/v1` | Base da API Sting9 |
| `STING9_TOKEN` / `STING9_API_KEY` | — | JWT do Sting9 (usado por `fetch_sting9`) |
| `STING9_EXPORT_PATH` | — | Export JSON já baixado (vazio = mais recente em `data/raw/`) |
| `KAGGLE_DATASET_SLUG` | — | `owner/dataset` do Kaggle para download via CLI |

## 🔁 Reprodutibilidade

Para reproduzir um experimento, registre no TCC:

- **Fonte dos dados**: o arquivo `data/raw/sting9_export_<data>.json` usado (+ seu `.meta.json`)
  e o CSV do Kaggle; `data/dataset_stats.json` guarda contagens, fontes e o `sha256` do
  `dataset.csv` consolidado.
- **Semente**: `ML_RANDOM_STATE` (default 42) — fixa o split 80/20, o `StratifiedKFold` e o
  Random Forest.
- **Parâmetros**: NB e RF nos defaults do scikit-learn; versão do `scikit-learn` fica gravada
  em `models/metrics.json`.
- **Resultados**: `models/cv_metrics.json` (validação cruzada) e `models/metrics.json` +
  `models/METRICS.md` (métricas no conjunto de teste).

## 🧪 Testes

```bash
pytest
```

## 🔗 Integração com o Backend

O backend .NET consome este serviço através de `IEmailClassificationService`
(`backend/PhishingDetectionAPI/Application/Services/ApplicationServices.cs`), que já chama
`POST {MlServiceSettings.BaseUrl}/classify` via cliente Refit (`IMlServiceClient`) e mapeia a
resposta JSON para `ClassificationResponse`, com fallback conservador (`suspicious`) caso o
microserviço esteja indisponível.
