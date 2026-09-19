importScripts("config.js");

// As chamadas HTTP ficam aqui no service worker de proposito: a partir do
// popup ou do content script, o Chrome bloquearia uma chamada http:// feita
// de uma pagina https:// (mixed content).

async function lerToken() {
  const dados = await chrome.storage.local.get(["token", "expiraEm", "usuario"]);
  if (!dados.token || !dados.expiraEm || Date.now() >= dados.expiraEm) return null;
  return dados;
}

async function login(username, password) {
  const resposta = await fetch(`${SERVIDOR}/api/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password })
  });

  const corpo = await resposta.json().catch(() => ({}));

  if (!resposta.ok) {
    throw new Error(corpo.message || "Usuario ou senha incorretos.");
  }

  // expiresIn vem em segundos; renovamos 1 minuto antes para nao expirar no meio de uma analise
  const validadeMs = Math.max((corpo.expiresIn || 3600) - 60, 60) * 1000;

  await chrome.storage.local.set({
    token: corpo.token,
    expiraEm: Date.now() + validadeMs,
    usuario: corpo.user?.fullName || corpo.user?.username || username
  });

  return { usuario: corpo.user?.fullName || corpo.user?.username || username };
}

async function cadastrar(dados) {
  const resposta = await fetch(`${SERVIDOR}/api/Users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      username: dados.username,
      email: dados.email,
      fullName: dados.fullName,
      department: dados.department || "",
      password: dados.password
    })
  });

  const corpo = await resposta.json().catch(() => ({}));

  if (!resposta.ok) {
    if (resposta.status === 409) throw new Error(corpo.message || "Usuario ou e-mail ja cadastrado.");
    throw new Error(primeiroErroDeValidacao(corpo) || "Nao foi possivel criar a conta.");
  }

  // Ja deixa a pessoa logada, para ela nao digitar a senha de novo
  return login(dados.username, dados.password);
}

function primeiroErroDeValidacao(corpo) {
  if (corpo.message) return corpo.message;
  const erros = corpo.errors && Object.values(corpo.errors)[0];
  return Array.isArray(erros) ? erros[0] : erros;
}

async function classificar(email) {
  const sessao = await lerToken();
  if (!sessao) {
    const erro = new Error("Sessao expirada. Entre novamente.");
    erro.precisaLogin = true;
    throw erro;
  }

  const resposta = await fetch(`${SERVIDOR}/api/Emails/classify`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${sessao.token}`
    },
    body: JSON.stringify({
      subject: email.subject,
      body: email.body,
      sender: email.sender,
      recipients: email.recipients || "",
      headers: null
    })
  });

  if (resposta.status === 401) {
    await chrome.storage.local.remove(["token", "expiraEm", "usuario"]);
    const erro = new Error("Sessao expirada. Entre novamente.");
    erro.precisaLogin = true;
    throw erro;
  }

  const corpo = await resposta.json().catch(() => ({}));

  if (!resposta.ok) {
    throw new Error(corpo.message || "Nao foi possivel analisar o e-mail.");
  }

  return corpo;
}

function traduzErroDeRede(erro) {
  if (erro instanceof TypeError) {
    return new Error(
      `Nao foi possivel falar com o servidor (${SERVIDOR}). ` +
        "Verifique se o backend esta ligado e se voce esta na rede da empresa."
    );
  }
  return erro;
}

chrome.runtime.onMessage.addListener((mensagem, _remetente, responder) => {
  (async () => {
    try {
      switch (mensagem.tipo) {
        case "status": {
          const sessao = await lerToken();
          responder({ ok: true, logado: !!sessao, usuario: sessao?.usuario, servidor: SERVIDOR });
          break;
        }
        case "login": {
          const resultado = await login(mensagem.username, mensagem.password);
          responder({ ok: true, ...resultado });
          break;
        }
        case "cadastrar": {
          const resultado = await cadastrar(mensagem.dados);
          responder({ ok: true, ...resultado });
          break;
        }
        case "logout": {
          await chrome.storage.local.remove(["token", "expiraEm", "usuario"]);
          responder({ ok: true });
          break;
        }
        case "classificar": {
          const resultado = await classificar(mensagem.email);
          responder({ ok: true, resultado });
          break;
        }
        default:
          responder({ ok: false, erro: "Acao desconhecida." });
      }
    } catch (erro) {
      const tratado = traduzErroDeRede(erro);
      responder({ ok: false, erro: tratado.message, precisaLogin: !!erro.precisaLogin });
    }
  })();

  return true; // resposta assincrona
});
