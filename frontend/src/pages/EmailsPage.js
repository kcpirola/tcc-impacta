import { useEffect, useState } from "react";
import EmailTable from "../components/EmailTable";
import { getEmailsByClassification, submitFeedback } from "../services/emailService";
import "./EmailsPage.css";

const TABS = [
  { value: "safe", label: "Seguros" },
  { value: "suspicious", label: "Suspeitos" },
  { value: "phishing", label: "Phishing" },
];

export default function EmailsPage() {
  const [activeTab, setActiveTab] = useState("phishing");
  const [emails, setEmails] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    loadEmails(activeTab);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  async function loadEmails(classification) {
    setLoading(true);
    setError("");
    try {
      const data = await getEmailsByClassification(classification, 0, 20);
      setEmails(data);
    } catch (err) {
      setError("Não foi possível carregar os e-mails.");
    } finally {
      setLoading(false);
    }
  }

  async function handleFeedback(emailId, feedback) {
    try {
      await submitFeedback(emailId, feedback);
      await loadEmails(activeTab);
    } catch (err) {
      setError("Não foi possível enviar o feedback.");
    }
  }

  return (
    <div className="emails-page">
      <h1>E-mails Processados</h1>

      <div className="emails-tabs">
        {TABS.map((tab) => (
          <button
            key={tab.value}
            type="button"
            className={activeTab === tab.value ? "active" : ""}
            onClick={() => setActiveTab(tab.value)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {error && <div className="emails-error">{error}</div>}

      {loading ? <p>Carregando...</p> : <EmailTable emails={emails} onFeedback={handleFeedback} />}
    </div>
  );
}
