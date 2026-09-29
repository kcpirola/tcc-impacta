import { useEffect, useState } from "react";
import StatCard from "../components/StatCard";
import EmailTable from "../components/EmailTable";
import { getEmailsByClassification, getStatistics, submitFeedback } from "../services/emailService";
import "./DashboardPage.css";

export default function DashboardPage() {
  const [statistics, setStatistics] = useState({ safe: 0, suspicious: 0, phishing: 0 });
  const [recentPhishing, setRecentPhishing] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    loadDashboardData();
  }, []);

  async function loadDashboardData() {
    setLoading(true);
    setError("");
    try {
      // Os cartões vêm da contagem no banco; a tabela, da primeira página de phishing.
      const [stats, phishing] = await Promise.all([
        getStatistics(),
        getEmailsByClassification("phishing", 0, 10),
      ]);
      setStatistics(stats);
      setRecentPhishing(phishing);
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

  return (
    <div className="dashboard-page">
      <h1>Dashboard</h1>

      {error && <div className="dashboard-error">{error}</div>}

      <div className="dashboard-stats">
        <StatCard title="E-mails Seguros" value={statistics.safe} variant="safe" />
        <StatCard title="E-mails Suspeitos" value={statistics.suspicious} variant="suspicious" />
        <StatCard title="E-mails Phishing" value={statistics.phishing} variant="phishing" />
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
