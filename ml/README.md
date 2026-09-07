# Microserviço de Machine Learning - Detecção de Phishing

Serviço Python responsável por classificar e-mails em **safe**, **suspicious** ou **phishing**,
usando um ensemble de **Naive Bayes** (triagem inicial) e **Random Forest** (classificação final),
combinando features de **TF-IDF** com indicadores heurísticos estruturais.

## 🏗️ Estrutura

```
ml/
├── config.py                          # Configurações centralizadas (paths, pesos do ensemble, treino)
├── requirements.txt                   # Dependências Python
├── training/
│   ├── data_preprocessing.py         # Limpeza e normalização de texto (SRP)
│   ├── feature_extraction.py         # Indicadores heurísticos (URLs, urgência, etc.)
│   ├── train_model.py                # Treinamento com validação cruzada k-fold
│   ├── evaluate_model.py             # Avaliação de métricas vs. metas do projeto
│   └── generate_sample_dataset.py    # Dataset sintético para testar o pipeline
├── service/
│   ├── classification_service.py     # Lógica de inferência do ensemble
│   └── app.py                        # API Flask consumida pelo backend .NET
├── models/                            # Modelos treinados (.pkl, não versionados)
└── data/                              # Datasets (não versionados)
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

### 2. Gerar dataset de teste (opcional, para smoke test)

```bash
python -m training.generate_sample_dataset
```

Ou coloque seu dataset real (Sting9 + Kaggle) em `data/dataset.csv` — ver [data/README.md](data/README.md).

### 3. Treinar os modelos

```bash
python -m training.train_model --dataset data/dataset.csv
```

Isso gera em `models/`:
- `naive_bayes_model.pkl`
- `random_forest_model.pkl`
- `tfidf_vectorizer.pkl`
- `feature_scaler.pkl`
- `label_encoder.pkl`

### 4. Avaliar os modelos

```bash
python -m training.evaluate_model --dataset data/dataset.csv
```

Compara acurácia, precisão, recall e F1-score com as metas do projeto (>95%, >94%, >96%, >95%).

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
