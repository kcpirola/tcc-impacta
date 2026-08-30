# Guia do Usuário - Sistema de Detecção de Phishing

**Data:** 30 de Agosto de 2026
**Versão:** 1.0
**Público-alvo:** Colaboradores e analistas de segurança da Health2B Hospitalar Ltda.

---

## 1. O que é o sistema?

O sistema analisa automaticamente os e-mails corporativos recebidos e os classifica em
três categorias:

| Classificação | O que significa |
|----------------|------------------|
| 🟢 **Seguro** | Nenhum sinal de phishing detectado |
| 🟡 **Suspeito** | Alguns indicadores de risco encontrados — revise com atenção |
| 🔴 **Phishing** | Alta probabilidade de ser uma tentativa de fraude/ataque |

Cada classificação vem acompanhada de um **percentual de confiança** e da lista de
**indicadores** que levaram àquela decisão (ex.: URL suspeita, linguagem urgente,
remetente com domínio divergente).

## 2. Como acessar

1. Abra o navegador e acesse o endereço informado pela equipe de TI
   (ex.: `http://servidor-interno:3000`).
2. Informe seu **usuário** e **senha** fornecidos pelo administrador do sistema.
3. Clique em **Entrar**.

> Em caso de esquecimento de senha, contate o administrador (papel `Admin`) — não há
> recuperação automática de senha por e-mail nesta versão.

## 3. Telas do Sistema

### 3.1 Dashboard

Tela inicial após o login. Mostra:
- Cartões com o total de e-mails **Seguros**, **Suspeitos** e **Phishing**.
- Lista dos e-mails de phishing mais recentes, com opção de dar feedback diretamente.

### 3.2 E-mails

Lista completa de e-mails processados, organizados em abas (**Seguros**, **Suspeitos**,
**Phishing**). Para cada e-mail você pode:
- Ver o remetente, assunto, classificação, confiança e data de recebimento.
- Dar **feedback** clicando em um dos três botões:
  - ✔️ **Classificação correta** — confirma que o sistema acertou.
  - ❌ **Classificação incorreta** — informa que o sistema errou.
  - ❓ **Não tenho certeza** — marca como incerto para revisão posterior.

> Seu feedback é usado para melhorar o modelo de Machine Learning ao longo do tempo.

### 3.3 Classificar E-mail (teste manual)

Permite colar manualmente o remetente, assunto e corpo de um e-mail suspeito (por
exemplo, recebido fora do fluxo automático) e obter uma classificação imediata, com
os indicadores de risco encontrados e as URLs suspeitas destacadas.

## 4. Entendendo os Indicadores

| Indicador | O que significa |
|-----------|------------------|
| `suspicious_url` | O e-mail contém uma URL suspeita (IP direto, encurtador, domínio incomum) |
| `urgent_language` | Uso de termos que induzem urgência ("clique aqui", "verifique agora") |
| `sender_domain_mismatch` | O domínio do remetente não bate com os links do corpo do e-mail |
| `generic_greeting` | Saudação genérica ("Caro cliente"), comum em campanhas de phishing em massa |

## 5. Boas Práticas de Segurança

- **Nunca** clique em links de e-mails classificados como Suspeito ou Phishing sem
  antes confirmar com a equipe de TI.
- Mesmo e-mails classificados como Seguro merecem atenção — nenhum sistema automático
  tem 100% de acerto. Em caso de dúvida, use o feedback ❓.
- Não compartilhe sua senha de acesso ao sistema com outros colaboradores.

## 6. Seus Direitos (LGPD)

O sistema processa dados de e-mails corporativos para fins de segurança. Você tem
direito a:
- Solicitar informações sobre quais dados seus estão armazenados.
- Solicitar a **exclusão de dados** (direito ao esquecimento) — peça ao administrador
  para abrir uma solicitação de exclusão em seu nome.
- Os dados são retidos por no máximo o período configurado pela empresa (padrão:
  90 dias) e podem ser anonimizados automaticamente após esse prazo.

## 7. Perguntas Frequentes (FAQ)

**O sistema bloqueia e-mails de phishing automaticamente?**
Não. O sistema **classifica e alerta**; a decisão de bloquear, quarentenar ou excluir
o e-mail continua sendo do administrador de e-mail da empresa.

**Por que um e-mail legítimo foi marcado como Suspeito?**
Alguns e-mails legítimos usam linguagem de urgência ou links encurtados por motivos
válidos (ex.: marketing, ferramentas de terceiros). Use o botão ❌ para informar o erro
e ajudar o sistema a aprender.

**Posso ver e-mails de outros departamentos?**
Isso depende do seu papel no sistema (`User`, `Analyst`, `Admin`), configurado pelo
administrador.

**O sistema funciona offline?**
Não. Ele depende do backend, do microserviço de Machine Learning e do banco de dados
estarem em execução no servidor da empresa (mas nenhum dado sai para a internet/nuvem
pública).

## 8. Suporte

Em caso de dúvidas técnicas ou problemas de acesso, contate a equipe de TI/administrador
do sistema. Para detalhes técnicos de operação, consulte o
[Guia de Deployment](DEPLOYMENT_GUIDE.md).
