const el = (id) => document.getElementById(id);

const VEREDITOS = {
  safe: "E-mail seguro",
  suspicious: "E-mail suspeito",
  phishing: "Phishing detectado"
};

function mostrarErro(texto) {
  el("erro").textContent = texto;
  el("erro").hidden = false;
}

function limparAvisos() {
  el("erro").hidden = true;
  el("resultado").hidden = true;
}

async function enviarAoFundo(mensagem) {
  return chrome.runtime.sendMessage(mensagem);
}

let mostrandoCadastro = false;

async function pintarTela() {
  const status = await enviarAoFundo({ tipo: "status" });
  const logado = status?.logado;

  el("tela-login").hidden = logado || mostrandoCadastro;
  el("tela-cadastro").hidden = logado || !mostrandoCadastro;
  el("tela-analise").hidden = !logado;
  el("sair").hidden = !logado;
  el("dashboard").hidden = !logado;
  el("rodape").textContent = `Servidor: ${status?.servidor ?? "nao configurado"}`;

  if (logado) {
    mostrandoCadastro = false;
    el("saudacao").textContent = `Conectado como ${status.usuario}.`;
  }
}

function trocarTela(paraCadastro) {
  mostrandoCadastro = paraCadastro;
  limparAvisos();
  pintarTela();
}

el("ir-cadastro").addEventListener("click", () => trocarTela(true));
el("ir-login").addEventListener("click", () => trocarTela(false));

el("form-cadastro").addEventListener("submit", async (evento) => {
  evento.preventDefault();
  limparAvisos();

  if (el("c-senha").value !== el("c-senha2").value) {
    mostrarErro("As duas senhas digitadas nao sao iguais.");
    return;
  }

  const botao = evento.target.querySelector("button[type=submit]");
  botao.disabled = true;
  botao.textContent = "Criando...";

  const resposta = await enviarAoFundo({
    tipo: "cadastrar",
    dados: {
      username: el("c-usuario").value.trim(),
      email: el("c-email").value.trim(),
      fullName: el("c-nome").value.trim(),
      department: el("c-departamento").value.trim(),
      password: el("c-senha").value
    }
  });

  botao.disabled = false;
  botao.textContent = "Criar conta";

  if (!resposta?.ok) {
    mostrarErro(resposta?.erro || "Nao foi possivel criar a conta.");
    return;
  }

  evento.target.reset();
  mostrandoCadastro = false;
  await pintarTela();
});

el("form-login").addEventListener("submit", async (evento) => {
  evento.preventDefault();
  limparAvisos();

  const botao = evento.target.querySelector("button");
  botao.disabled = true;
  botao.textContent = "Entrando...";

  const resposta = await enviarAoFundo({
    tipo: "login",
    username: el("usuario").value.trim(),
    password: el("senha").value
  });

  botao.disabled = false;
  botao.textContent = "Entrar";

  if (!resposta?.ok) {
    mostrarErro(resposta?.erro || "Nao foi possivel entrar.");
    return;
  }

  el("senha").value = "";
  await pintarTela();
});

el("sair").addEventListener("click", async () => {
  await enviarAoFundo({ tipo: "logout" });
  limparAvisos();
  await pintarTela();
});

el("dashboard").addEventListener("click", () => {
  chrome.tabs.create({ url: `${PAINEL_URL}/dashboard` });
});

// O Chrome fecha o popup sempre que ele perde o foco, e nao ha como evitar isso.
// Por isso o texto colado a mao e guardado a cada tecla e devolvido na reabertura.
// Fica em storage.session: vive na memoria e desaparece ao fechar o navegador,
// para o conteudo do e-mail nao ficar gravado em disco.
const CAMPOS_RASCUNHO = ["m-remetente", "m-assunto", "m-corpo"];

async function salvarRascunho() {
  const rascunho = { aberto: el("modo-manual").open };
  for (const id of CAMPOS_RASCUNHO) rascunho[id] = el(id).value;
  await chrome.storage.session.set({ rascunho });
}

async function restaurarRascunho() {
  const { rascunho } = await chrome.storage.session.get("rascunho");
  if (!rascunho) return;

  for (const id of CAMPOS_RASCUNHO) el(id).value = rascunho[id] ?? "";
  el("modo-manual").open = rascunho.aberto || CAMPOS_RASCUNHO.some((id) => el(id).value !== "");
}

async function descartarRascunho() {
  for (const id of CAMPOS_RASCUNHO) el(id).value = "";
  await chrome.storage.session.remove("rascunho");
}

for (const id of CAMPOS_RASCUNHO) {
  el(id).addEventListener("input", salvarRascunho);
}
el("modo-manual").addEventListener("toggle", salvarRascunho);

async function lerEmailDaAba() {
  const [aba] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (!aba?.id) return null;

  try {
    const resposta = await chrome.tabs.sendMessage(aba.id, { tipo: "extrair" });
    return resposta?.ok ? resposta.email : null;
  } catch {
    // content script nao esta nesta aba (nao e Gmail nem Outlook Web)
    return null;
  }
}

function exibirResultado(dados) {
  const classe = String(dados.classification || "").toLowerCase();

  const veredito = el("veredito");
  veredito.textContent = VEREDITOS[classe] || dados.classification;
  veredito.className = classe;

  // A API devolve a confianca ja em percentual (ex.: 64.04), nao em fracao.
  const confianca = Number(dados.confidence);
  el("confianca").textContent = Number.isFinite(confianca)
    ? `Confianca do modelo: ${Math.round(confianca)}%`
    : "";

  el("recomendacao").textContent = dados.recommendation || "";

  const indicadores = dados.indicators || [];
  el("rotulo-indicadores").hidden = indicadores.length === 0;
  const lista = el("indicadores");
  lista.textContent = "";
  for (const indicador of indicadores) {
    const item = document.createElement("li");
    item.textContent = indicador.description || indicador.type;
    lista.appendChild(item);
  }

  const urls = dados.suspiciousUrls || [];
  el("urls-bloco").hidden = urls.length === 0;
  const listaUrls = el("urls");
  listaUrls.textContent = "";
  for (const url of urls) {
    const item = document.createElement("li");
    item.textContent = url;
    listaUrls.appendChild(item);
  }

  el("resultado").hidden = false;
}

async function analisar(email) {
  limparAvisos();

  if (!email?.subject || !email?.body || !email?.sender) {
    mostrarErro(
      "Nao consegui ler o e-mail da tela. Abra um e-mail no Gmail ou Outlook Web, " +
        'ou use "Nao achou o e-mail? Cole aqui".'
    );
    el("modo-manual").open = true;
    return false;
  }

  el("carregando").hidden = false;
  const resposta = await enviarAoFundo({ tipo: "classificar", email });
  el("carregando").hidden = true;

  if (!resposta?.ok) {
    mostrarErro(resposta?.erro || "Nao foi possivel analisar o e-mail.");
    if (resposta?.precisaLogin) await pintarTela();
    return false;
  }

  exibirResultado(resposta.resultado);
  return true;
}

el("analisar").addEventListener("click", async () => {
  analisar(await lerEmailDaAba());
});

el("analisar-manual").addEventListener("click", async () => {
  const analisou = await analisar({
    sender: el("m-remetente").value.trim(),
    subject: el("m-assunto").value.trim(),
    body: el("m-corpo").value.trim(),
    recipients: ""
  });

  // Só descarta o rascunho depois de a análise dar certo; se falhar, o texto continua lá.
  if (analisou) await descartarRascunho();
});

pintarTela();
restaurarRascunho();
