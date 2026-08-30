# Datasets de Treinamento

Esta pasta armazena os datasets utilizados para treinar os modelos de classificação de phishing.
Os arquivos de dados **não são versionados no Git** (ver `.gitignore`) devido ao tamanho e a
possíveis restrições de licenciamento.

## Formato Esperado

Os scripts de treinamento (`training/train_model.py`) esperam um arquivo CSV com as colunas:

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `subject` | string | Assunto do e-mail |
| `body` | string | Corpo do e-mail (texto ou HTML) |
| `sender` | string | Endereço de e-mail do remetente |
| `label` | string | Uma de: `safe`, `suspicious`, `phishing` |

Exemplo (`dataset.csv`):

```csv
subject,body,sender,label
"Reunião de equipe amanhã","Olá, confirmando nossa reunião às 10h.",joao@empresa.com,safe
"URGENTE: Verifique sua conta agora","Clique aqui para verificar: http://192.168.1.1/login",security@bank-alert.tk,phishing
```

## Fontes de Dados

- **Sting9 Research Initiative**: E-mails de phishing reais reportados por usuários corporativos.
- **Kaggle Phishing Dataset**: Dataset público de e-mails rotulados para pesquisa acadêmica.

Baixe os datasets manualmente e salve como `data/dataset.csv`, ou combine múltiplas fontes
em um único CSV seguindo o formato acima.

## Dataset Sintético para Testes

Para validar o pipeline (pré-processamento → treino → avaliação → inferência) sem depender
de dados reais, use o gerador de dados sintéticos:

```bash
python -m training.generate_sample_dataset
```

Isso cria `data/dataset.csv` com exemplos artificiais de e-mails seguros, suspeitos e phishing,
suficientes para testar o pipeline de ponta a ponta. **Não use este dataset para métricas reais.**
