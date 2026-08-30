import "./StatCard.css";

/** Cartão de estatística usado no dashboard (ex.: total de e-mails phishing). */
export default function StatCard({ title, value, variant = "default" }) {
  return (
    <div className={`stat-card stat-card-${variant}`}>
      <span className="stat-card-title">{title}</span>
      <span className="stat-card-value">{value}</span>
    </div>
  );
}
