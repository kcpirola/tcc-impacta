import ClassificationBadge from "./ClassificationBadge";
import "./EmailTable.css";

/**
 * Tabela de e-mails processados, com ação de feedback do usuário.
 * @param {Array<object>} emails Lista de EmailDto retornados pelo backend
 * @param {(emailId: number, feedback: string) => void} onFeedback
 */
export default function EmailTable({ emails, onFeedback }) {
  if (!emails || emails.length === 0) {
    return <p className="email-table-empty">Nenhum e-mail encontrado.</p>;
  }

  return (
    <table className="email-table">
      <thead>
        <tr>
          <th>Remetente</th>
          <th>Assunto</th>
          <th>Classificação</th>
          <th>Confiança</th>
          <th>Recebido em</th>
          <th>Feedback</th>
        </tr>
      </thead>
      <tbody>
        {emails.map((email) => (
          <tr key={email.id}>
            <td>{email.sender}</td>
            <td>{email.subject}</td>
            <td>
              <ClassificationBadge classification={email.classification} />
            </td>
            <td>{email.confidence}%</td>
            <td>{new Date(email.receivedDate).toLocaleString("pt-BR")}</td>
            <td className="email-table-actions">
              <button type="button" onClick={() => onFeedback(email.id, "correct")} title="Classificação correta">
                ✔️
              </button>
              <button type="button" onClick={() => onFeedback(email.id, "incorrect")} title="Classificação incorreta">
                ❌
              </button>
              <button type="button" onClick={() => onFeedback(email.id, "uncertain")} title="Não tenho certeza">
                ❓
              </button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
