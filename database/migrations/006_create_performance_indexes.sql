-- Migration 006: Índices de performance
-- Requer: 001 a 005 (todas as tabelas já devem existir)

CREATE INDEX IF NOT EXISTS idx_emails_sender ON emails(sender);
CREATE INDEX IF NOT EXISTS idx_emails_classification ON emails(classification);
CREATE INDEX IF NOT EXISTS idx_emails_received_date ON emails(received_date);
CREATE INDEX IF NOT EXISTS idx_emails_processed_date ON emails(processed_date);
CREATE INDEX IF NOT EXISTS idx_phishing_indicators_email_id ON phishing_indicators(email_id);
CREATE INDEX IF NOT EXISTS idx_urls_detected_email_id ON urls_detected(email_id);
CREATE INDEX IF NOT EXISTS idx_attachments_email_id ON attachments(email_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
