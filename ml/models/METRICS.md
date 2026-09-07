# Relatório de métricas — classificação de phishing

Gerado em 2026-09-07T23:01:26.539257+00:00 · scikit-learn 1.9.0

Dataset: `data\dataset.csv` · 56077 registros · split de teste 20% · validação cruzada k=10

## Distribuição de classes

| Classe | Registros |
|---|---|
| phishing | 7817 |
| safe | 26419 |
| suspicious | 21841 |

## Resultados (média ponderada)

| Modelo | Acurácia | Precisão | Recall | F1 |
|---|---|---|---|---|
| Naive Bayes | 0.9280 | 0.9306 | 0.9280 | 0.9278 |
| Random Forest | 0.9744 | 0.9746 | 0.9744 | 0.9739 |
| Ensemble (NB + RF) | 0.9715 | 0.9715 | 0.9715 | 0.9711 |

Metas do projeto: acurácia > 0.95, precisão > 0.94, recall > 0.96, F1 > 0.95.

## Matriz de confusão — Ensemble

Linhas = classe verdadeira, colunas = classe prevista.

| verdadeiro \ previsto | phishing | safe | suspicious |
|---|---|---|---|
| phishing | 1361 | 176 | 27 |
| safe | 18 | 5254 | 12 |
| suspicious | 52 | 35 | 4281 |

## Métricas por classe — Ensemble

| Classe | Precisão | Recall | F1 | Suporte |
|---|---|---|---|---|
| phishing | 0.9511 | 0.8702 | 0.9088 | 1564 |
| safe | 0.9614 | 0.9943 | 0.9776 | 5284 |
| suspicious | 0.9910 | 0.9801 | 0.9855 | 4368 |
