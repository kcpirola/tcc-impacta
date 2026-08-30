-- Migration 007: Views para relatórios (dashboard, Power BI)
-- Requer: 001 a 005

CREATE OR REPLACE VIEW v_email_classification_summary AS
SELECT
    classification,
    COUNT(*) as total,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM emails), 2) as percentage,
    AVG(confidence) as avg_confidence,
    MAX(processed_date) as last_processed
FROM emails
GROUP BY classification
ORDER BY total DESC;

CREATE OR REPLACE VIEW v_suspicious_senders AS
SELECT
    sender,
    COUNT(*) as phishing_count,
    COUNT(CASE WHEN classification = 'phishing' THEN 1 END) as confirmed_phishing,
    MAX(processed_date) as last_occurrence
FROM emails
WHERE classification IN ('suspicious', 'phishing')
GROUP BY sender
ORDER BY phishing_count DESC;

CREATE OR REPLACE VIEW v_model_performance_summary AS
SELECT
    model_name,
    MAX(model_version) as latest_version,
    AVG(accuracy) as avg_accuracy,
    AVG(precision) as avg_precision,
    AVG(recall) as avg_recall,
    AVG(f1_score) as avg_f1_score,
    MAX(tested_at) as last_test_date
FROM model_performance
GROUP BY model_name;

COMMENT ON TABLE emails IS 'Armazena todos os e-mails processados pelo sistema';
COMMENT ON TABLE phishing_indicators IS 'Armazena indicadores detectados que levaram à classificação';
COMMENT ON TABLE audit_logs IS 'Log de todas as ações dos usuários (conformidade)';
COMMENT ON TABLE model_performance IS 'Métricas de desempenho dos modelos ML';
