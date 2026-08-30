import { useEffect, useState } from "react";
import StatCard from "../components/StatCard";
import EmailTable from "../components/EmailTable";
import { getEmailsByClassification, submitFeedback } from "../services/emailService";
import "./DashboardPage.css";

const CLASSIFICATIONS = ["safe", "suspicious", "phishing"];

export default function DashboardPage() {
  const [emailsByClassification, setEmailsByClassification] = useState({
    safe: [],
    suspicious: [],
    phishing: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    loadDashboardData();
  }, []);

  async function loadDashboardData() {
    setLoading(true);
    setError("");
    try {
      const results = await Promise.all(
        CLASSIFICATIONS.map((classification) => getEmailsByClassification(classification, 0, 10))
      );
      setEmailsByClassification({
        safe: results[0],
        suspicious: results[1],
        phishing: results[2],
      });
    } catch (err) {
      setError("Não foi possível carregar os dados do dashboard.");
    } finally {
      setLoading(false);
    }
  }

  async function handleFeedback(emailId, feedback) {
    try {
      await submitFeedback(emailId, feedback);
      await loadDashboardData();
    } catch (err) {
      setError("Não foi possível enviar o feedback.");
    }
  }

  const recentPhishing = emailsByClassification.phishing;

  return (
    <div className="dashboard-page">
      <h1>Dashboard</h1>

      {error && <div className="dashboard-error">{error}</div>}

      <div className="dashboard-stats">
        <StatCard title="E-mails Seguros" value={emailsByClassification.safe.length} variant="safe" />
        <StatCard title="E-mails Suspeitos" value={emailsByClassification.suspicious.length} variant="suspicious" />
        <StatCard title="E-mails Phishing" value={emailsByClassification.phishing.length} variant="phishing" />
      </div>

      <section className="dashboard-section">
        <h2>E-mails de Phishing Recentes</h2>
        {loading ? (
          <p>Carregando...</p>
        ) : (
          <EmailTable emails={recentPhishing} onFeedback={handleFeedback} />
        )}
      </section>
    </div>
  );
}
