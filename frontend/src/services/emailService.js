import api from "./api";

/**
 * Classifica um único e-mail como safe, suspicious ou phishing.
 * @param {{subject: string, body: string, sender: string, recipients?: string}} email
 * @returns {Promise<object>} Resposta da classificação (ClassificationResponse)
 */
export async function classifyEmail(email) {
  const { data } = await api.post("/emails/classify", email);
  return data;
}

/**
 * Classifica múltiplos e-mails em lote.
 * @param {Array<object>} emails
 * @returns {Promise<Array<object>>}
 */
export async function classifyEmailsBatch(emails) {
  const { data } = await api.post("/emails/classify-batch", emails);
  return data;
}

/**
 * Envia o feedback do usuário sobre a classificação de um e-mail.
 * @param {number} emailId
 * @param {"correct"|"incorrect"|"uncertain"} feedback
 */
export async function submitFeedback(emailId, feedback) {
  const { data } = await api.post("/emails/feedback", { emailId, feedback });
  return data;
}

/**
 * Busca e-mails por classificação com paginação.
 * @param {"safe"|"suspicious"|"phishing"} classification
 * @param {number} skip
 * @param {number} take
 * @returns {Promise<Array<object>>}
 */
export async function getEmailsByClassification(classification, skip = 0, take = 10) {
  const { data } = await api.get(`/emails/by-classification/${classification}`, {
    params: { skip, take },
  });
  return data;
}
