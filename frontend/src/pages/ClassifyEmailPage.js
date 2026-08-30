import { useState } from "react";
import ClassificationBadge from "../components/ClassificationBadge";
import { classifyEmail } from "../services/emailService";
import "./ClassifyEmailPage.css";

const initialForm = { subject: "", body: "", sender: "", recipients: "" };

export default function ClassifyEmailPage() {
  const [form, setForm] = useState(initialForm);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setLoading(true);
    setError("");
    setResult(null);

    try {
      const response = await classifyEmail(form);
      setResult(response);
    } catch (err) {
      setError(err.response?.data?.message || "Não foi possível classificar o e-mail.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="classify-page">
      <h1>Classificar E-mail</h1>

      <form className="classify-form" onSubmit={handleSubmit}>
        <label htmlFor="sender">Remetente</label>
        <input id="sender" name="sender" value={form.sender} onChange={handleChange} required />

        <label htmlFor="recipients">Destinatários</label>
        <input id="recipients" name="recipients" value={form.recipients} onChange={handleChange} />

        <label htmlFor="subject">Assunto</label>
        <input id="subject" name="subject" value={form.subject} onChange={handleChange} required />

        <label htmlFor="body">Corpo do e-mail</label>
        <textarea id="body" name="body" rows={8} value={form.body} onChange={handleChange} required />

        <button type="submit" disabled={loading}>
          {loading ? "Classificando..." : "Classificar"}
        </button>
      </form>

      {error && <div className="classify-error">{error}</div>}

      {result && (
        <div className="classify-result">
          <div className="classify-result-header">
            <ClassificationBadge classification={result.classification} />
            <span>{result.confidence}% de confiança</span>
          </div>
          <p className="classify-recommendation">{result.recommendation}</p>

          {result.indicators?.length > 0 && (
            <div className="classify-indicators">
              <h3>Indicadores detectados</h3>
              <ul>
                {result.indicators.map((indicator, index) => (
                  <li key={index}>
                    <strong>{indicator.type}:</strong> {indicator.description} (risco {indicator.riskScore})
                  </li>
                ))}
              </ul>
            </div>
          )}

          {result.suspiciousUrls?.length > 0 && (
            <div className="classify-urls">
              <h3>URLs suspeitas</h3>
              <ul>
                {result.suspiciousUrls.map((url, index) => (
                  <li key={index}>{url}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
