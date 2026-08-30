-- Migration 003: Auditoria e conformidade LGPD
-- Requer: 001_create_users_table.sql

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
