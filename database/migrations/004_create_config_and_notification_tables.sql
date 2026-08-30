-- Migration 004: Configurações do sistema e notificações
-- Requer: 001_create_users_table.sql, 002_create_emails_and_related_tables.sql

CREATE TABLE IF NOT EXISTS system_config (
    id SERIAL PRIMARY KEY,
    config_key VARCHAR(100) NOT NULL UNIQUE,
    config_value TEXT,
    description TEXT,
    data_type VARCHAR(50), -- 'string', 'integer', 'boolean'
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

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
