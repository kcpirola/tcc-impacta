import "./ClassificationBadge.css";

const LABELS = {
  safe: "Seguro",
  suspicious: "Suspeito",
  phishing: "Phishing",
};

/** Exibe a classificação de um e-mail com cor semântica (verde/amarelo/vermelho). */
export default function ClassificationBadge({ classification }) {
  const normalized = (classification || "").toLowerCase();
  const label = LABELS[normalized] || classification;

  return <span className={`badge badge-${normalized}`}>{label}</span>;
}
