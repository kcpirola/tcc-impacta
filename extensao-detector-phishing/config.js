// ===================================================================
//  LINHAS QUE PRECISAM SER EDITADAS ANTES DE DISTRIBUIR O ZIP
// ===================================================================
//
//  Troque pelo endereco do computador onde o backend .NET esta rodando.
//  NAO use "localhost" aqui: para o funcionario, localhost seria o PC
//  dele proprio, e o backend nao esta la.
//
//  Para descobrir o endereco, rode "ipconfig" no PC do servidor e
//  procure o "Endereco IPv4". Exemplo: 192.168.1.100
//
//  Exemplos validos:
//     "http://192.168.1.100:5001"
//     "https://phishing.empresa.com"
//
// ===================================================================

const SERVIDOR = "http://192.168.1.100:5000";

// Endereco do painel web (relatorio/dashboard). E o mesmo tipo de endereco
// acima, mas apontando para onde o frontend React esta publicado.
const PAINEL_URL = "http://192.168.1.100:3000";
