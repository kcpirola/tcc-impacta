-- ============================================
-- SCHEMA INICIAL - Sistema de Detecção de Phishing
-- ============================================
-- Data: 30/08/2026
-- Versão: 1.0
-- Descrição: Schema do banco PostgreSQL para o sistema
--
-- Ordem de execução recomendada:
--   1. psql -f database/schema.sql   (estrutura: tabelas, índices, triggers, views)
--   2. psql -f database/seed.sql     (dados iniciais: usuário admin, configurações)
--
-- Para deployments incrementais, ver database/migrations/ (scripts versionados
-- que, aplicados em sequência, reconstroem este mesmo schema).

-- ============================================
-- TABELAS PRINCIPAIS
-- ============================================

-- Tabela de Usuários
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255),
    department VARCHAR(100),
    role VARCHAR(50) NOT NULL DEFAULT 'user', -- admin, analyst, user
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

-- Tabela de E-mails Processados
CREATE TABLE IF NOT EXISTS emails (
    id SERIAL PRIMARY KEY,
    message_id VARCHAR(255) UNIQUE,
    subject VARCHAR(500),
    sender VARCHAR(255) NOT NULL,
    recipients VARCHAR(1000),
    received_date TIMESTAMP,
    processed_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    email_body TEXT,
    email_headers TEXT,
    classification VARCHAR(50) NOT NULL, -- 'safe', 'suspicious', 'phishing'
    confidence DECIMAL(5,2), -- 0.00 a 100.00
    is_validated BOOLEAN DEFAULT false,
    user_feedback VARCHAR(50), -- 'correct', 'incorrect', 'uncertain'
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Indicadores de Phishing Detectados
CREATE TABLE IF NOT EXISTS phishing_indicators (
    id SERIAL PRIMARY KEY,
    email_id INTEGER NOT NULL,
    indicator_type VARCHAR(100), -- 'suspicious_link', 'urgent_language', 'unknown_sender'
    indicator_value VARCHAR(500),
    risk_score DECIMAL(5,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE CASCADE
);

-- Tabela de URLs Detectadas
CREATE TABLE IF NOT EXISTS urls_detected (
    id SERIAL PRIMARY KEY,
    email_id INTEGER NOT NULL,
    url VARCHAR(2000),
    is_suspicious BOOLEAN,
    reputation_score DECIMAL(5,2),
    checked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE CASCADE
);

-- Tabela de Anexos
CREATE TABLE IF NOT EXISTS attachments (
    id SERIAL PRIMARY KEY,
    email_id INTEGER NOT NULL,
    file_name VARCHAR(500),
    file_size_bytes BIGINT,
    file_hash VARCHAR(255),
    is_malicious BOOLEAN,
    scan_result TEXT,
    scanned_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE CASCADE
);

-- ============================================
-- TABELAS DE AUDITORIA E CONFORMIDADE
-- ============================================

-- Tabela de Logs de Acesso
CREATE TABLE IF NOT EXISTS audit_logs (
    id SERIAL PRIMARY KEY,
    user_id INTEGER,
    action VARCHAR(100), -- 'login', 'view_email', 'export_report'
    resource_type VARCHAR(100),
    resource_id INTEGER,
    description TEXT,
    ip_address VARCHAR(45),
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Tabela de Consentimento LGPD
CREATE TABLE IF NOT EXISTS lgpd_consents (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    consent_type VARCHAR(100), -- 'data_processing', 'email_analysis'
    is_consented BOOLEAN DEFAULT false,
    consent_date TIMESTAMP,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Tabela de Requisições de Exclusão (Direito ao Esquecimento)
CREATE TABLE IF NOT EXISTS data_deletion_requests (
    id SERIAL PRIMARY KEY,
    user_id INTEGER,
    email_address VARCHAR(255),
    reason TEXT,
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'approved', 'completed', 'rejected'
    requested_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP,
    notes TEXT
);

-- ============================================
-- TABELAS DE CONFIGURAÇÃO
-- ============================================

-- Tabela de Configurações do Sistema
CREATE TABLE IF NOT EXISTS system_config (
    id SERIAL PRIMARY KEY,
    config_key VARCHAR(100) NOT NULL UNIQUE,
    config_value TEXT,
    description TEXT,
    data_type VARCHAR(50), -- 'string', 'integer', 'boolean'
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Notificações
CREATE TABLE IF NOT EXISTS notifications (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    email_id INTEGER,
    notification_type VARCHAR(50), -- 'phishing_detected', 'report_ready'
    title VARCHAR(200),
    message TEXT,
    is_read BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    read_at TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE SET NULL
);

-- ============================================
-- TABELAS DE MÉTRICAS
-- ============================================

-- Tabela de Desempenho do Modelo
CREATE TABLE IF NOT EXISTS model_performance (
    id SERIAL PRIMARY KEY,
    model_name VARCHAR(100), -- 'naive_bayes', 'random_forest'
    model_version VARCHAR(20),
    accuracy DECIMAL(5,4),
    precision DECIMAL(5,4),
    recall DECIMAL(5,4),
    f1_score DECIMAL(5,4),
    tested_emails INTEGER,
    tested_at TIMESTAMP,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de Estatísticas Diárias
CREATE TABLE IF NOT EXISTS daily_statistics (
    id SERIAL PRIMARY KEY,
    statistic_date DATE NOT NULL UNIQUE,
    total_emails_processed INTEGER DEFAULT 0,
    emails_classified_safe INTEGER DEFAULT 0,
    emails_classified_suspicious INTEGER DEFAULT 0,
    emails_classified_phishing INTEGER DEFAULT 0,
    avg_processing_time_ms DECIMAL(10,2),
    model_accuracy DECIMAL(5,4),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- ÍNDICES PARA PERFORMANCE
-- ============================================

CREATE INDEX idx_emails_sender ON emails(sender);
CREATE INDEX idx_emails_classification ON emails(classification);
CREATE INDEX idx_emails_received_date ON emails(received_date);
CREATE INDEX idx_emails_processed_date ON emails(processed_date);
CREATE INDEX idx_phishing_indicators_email_id ON phishing_indicators(email_id);
CREATE INDEX idx_urls_detected_email_id ON urls_detected(email_id);
CREATE INDEX idx_attachments_email_id ON attachments(email_id);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_notifications_user_id ON notifications(user_id);

-- ============================================
-- DADOS INICIAIS
-- ============================================
-- Os dados iniciais (usuário admin, configurações padrão) foram movidos
-- para database/seed.sql, para manter este arquivo restrito à estrutura (DDL).
-- Execute: psql -U app_user -d phishing_detection -f seed.sql

-- ============================================
-- CONSTRAINTS E REGRAS
-- ============================================

-- Criar trigger para atualizar updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_timestamp BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_emails_timestamp BEFORE UPDATE ON emails
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================
-- VIEWS PARA RELATÓRIOS
-- ============================================

-- View: Resumo de E-mails por Classificação
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

-- View: Remetentes Suspeitos
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

-- View: Estatísticas de Desempenho
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

-- ============================================
-- COMENTÁRIOS
-- ============================================

COMMENT ON TABLE emails IS 'Armazena todos os e-mails processados pelo sistema';
COMMENT ON TABLE phishing_indicators IS 'Armazena indicadores detectados que levaram à classificação';
COMMENT ON TABLE audit_logs IS 'Log de todas as ações dos usuários (conformidade)';
COMMENT ON TABLE model_performance IS 'Métricas de desempenho dos modelos ML';

-- ============================================
-- FIM DO SCHEMA
-- ============================================
