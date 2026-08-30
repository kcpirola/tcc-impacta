-- Migration 002: E-mails processados e entidades relacionadas (indicadores, URLs, anexos)
-- Requer: 001_create_users_table.sql

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

CREATE TABLE IF NOT EXISTS phishing_indicators (
    id SERIAL PRIMARY KEY,
    email_id INTEGER NOT NULL,
    indicator_type VARCHAR(100), -- 'suspicious_link', 'urgent_language', 'unknown_sender'
    indicator_value VARCHAR(500),
    risk_score DECIMAL(5,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS urls_detected (
    id SERIAL PRIMARY KEY,
    email_id INTEGER NOT NULL,
    url VARCHAR(2000),
    is_suspicious BOOLEAN,
    reputation_score DECIMAL(5,2),
    checked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (email_id) REFERENCES emails(id) ON DELETE CASCADE
);

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

CREATE TRIGGER update_emails_timestamp BEFORE UPDATE ON emails
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
