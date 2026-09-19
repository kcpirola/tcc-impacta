// Le o e-mail aberto na tela. Gmail e Outlook Web mudam seus seletores com
// alguma frequencia, entao cada campo tem varias tentativas e o popup ainda
// oferece colar o texto na mao se nada for encontrado.

function primeiroTexto(seletores) {
  for (const seletor of seletores) {
    const elemento = document.querySelector(seletor);
    const texto = elemento?.innerText?.trim();
    if (texto) return texto;
  }
  return "";
}

function extrairRemetente() {
  const comAtributo = document.querySelector(
    'span.gD[email], span[email]:not([email=""])'
  );
  if (comAtributo?.getAttribute("email")) return comAtributo.getAttribute("email");

  const porTitulo = document.querySelector('span[title*="@"], a[title*="@"]');
  if (porTitulo?.getAttribute("title")?.includes("@")) {
    return porTitulo.getAttribute("title").trim();
  }

  const texto = primeiroTexto(['[data-testid="SenderPersona"]', ".OZZZK", ".pT"]);
  const achado = texto.match(/[\w.+-]+@[\w-]+\.[\w.-]+/);
  return achado ? achado[0] : texto;
}

function extrairEmail() {
  const subject = primeiroTexto([
    "h2.hP",
    'div[role="main"] h2',
    'span[id*="subject"]',
    'div[role="heading"][aria-level="2"]',
    '[data-testid="SubjectLine"]'
  ]);

  const body = primeiroTexto([
    "div.a3s",
    'div[id*="UniqueMessageBody"]',
    'div[aria-label*="Corpo da mensagem"]',
    'div[aria-label*="Message body"]',
    'div[role="document"]'
  ]);

  return {
    subject,
    body,
    sender: extrairRemetente(),
    recipients: primeiroTexto(["span.g2[email]", 'div[aria-label*="Para"]']) || ""
  };
}

chrome.runtime.onMessage.addListener((mensagem, _remetente, responder) => {
  if (mensagem.tipo === "extrair") {
    try {
      responder({ ok: true, email: extrairEmail() });
    } catch (erro) {
      responder({ ok: false, erro: erro.message });
    }
  }
  return true;
});
