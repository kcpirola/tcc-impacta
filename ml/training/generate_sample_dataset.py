"""
Gera um dataset sintético pequeno para testar o pipeline de treinamento de ponta a ponta.
NÃO deve ser usado para calcular métricas reais de desempenho do modelo.

Uso:
    python -m training.generate_sample_dataset
"""
import csv
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent.parent))

from config import Config

SAFE_SAMPLES = [
    ("Reunião de equipe amanhã", "Olá, confirmando nossa reunião às 10h na sala 3.", "joao.silva@empresa.com"),
    ("Relatório mensal", "Segue em anexo o relatório de vendas do mês.", "maria.santos@empresa.com"),
    ("Férias aprovadas", "Suas férias foram aprovadas para o próximo mês.", "rh@empresa.com"),
    ("Atualização do projeto", "O projeto está progredindo conforme planejado.", "gerente@empresa.com"),
    ("Convite para almoço", "Vamos almoçar juntos na sexta-feira?", "colega@empresa.com"),
    ("Documentação atualizada", "A documentação técnica foi atualizada no repositório.", "dev@empresa.com"),
    ("Agenda da semana", "Segue a agenda de reuniões desta semana.", "secretaria@empresa.com"),
    ("Feedback do cliente", "O cliente enviou um feedback positivo sobre o produto.", "vendas@empresa.com"),
]

SUSPICIOUS_SAMPLES = [
    (
        "Confirme seus dados",
        "Prezado cliente, por favor confirme seus dados de acesso clicando no link abaixo.",
        "suporte@servico-externo.com",
    ),
    (
        "Atualização de segurança pendente",
        "Detectamos atividade incomum. Recomendamos verificar sua conta em breve.",
        "seguranca@empresa-parceira.com",
    ),
    (
        "Fatura em anexo",
        "Segue fatura em anexo, favor confirmar pagamento até amanhã.",
        "financeiro@fornecedor-novo.com",
    ),
]

PHISHING_SAMPLES = [
    (
        "URGENTE: Sua conta será suspensa",
        "Clique aqui imediatamente para verificar sua conta: http://192.168.10.5/login-bank",
        "security@bank-alert.tk",
    ),
    (
        "Verifique sua senha agora",
        "Sua senha expira hoje! Clique aqui: http://bit.ly/verify-now Caro cliente, aja agora.",
        "no-reply@account-verify.xyz",
    ),
    (
        "Prêmio exclusivo - Aja agora",
        "Parabéns! Você ganhou um prêmio. Clique aqui para resgatar: http://tinyurl.com/premio-vip",
        "promo@premios-vip.ga",
    ),
    (
        "Confirmação de conta necessária URGENTE",
        "Caro cliente, detectamos atividade incomum. Confirme sua conta agora: http://200.15.3.8/confirm",
        "alert@secure-verification.tk",
    ),
]


def generate() -> None:
    Config.ensure_directories()
    output_path = Config.DATA_DIR / "dataset.csv"

    rows = []
    for subject, body, sender in SAFE_SAMPLES:
        rows.append((subject, body, sender, "safe"))
    for subject, body, sender in SUSPICIOUS_SAMPLES:
        rows.append((subject, body, sender, "suspicious"))
    for subject, body, sender in PHISHING_SAMPLES:
        rows.append((subject, body, sender, "phishing"))

    with open(output_path, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(["subject", "body", "sender", "label"])
        writer.writerows(rows)

    print(f"Dataset sintético gerado em: {output_path}")
    print(f"Total de exemplos: {len(rows)} (use apenas para testar o pipeline)")


if __name__ == "__main__":
    generate()
