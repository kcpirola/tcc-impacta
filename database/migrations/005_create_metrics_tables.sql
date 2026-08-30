-- Migration 005: Métricas de modelo ML e estatísticas diárias
-- Requer: nenhuma dependência de tabela (mas segue a ordem lógica do schema)

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
