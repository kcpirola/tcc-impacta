-- ============================================
-- SEED - Dados Iniciais do Sistema de Detecção de Phishing
-- ============================================
-- Pré-requisito: execute schema.sql antes deste script.
-- Uso: psql -U app_user -d phishing_detection -f seed.sql

-- ============================================
-- USUÁRIO ADMINISTRADOR PADRÃO
-- ============================================
-- ATENÇÃO: troque a senha imediatamente após o primeiro login.
-- Hash bcrypt abaixo corresponde à senha "admin123" (apenas para ambiente local/dev).
INSERT INTO users (username, email, password_hash, full_name, department, role, is_active)
VALUES (
    'admin',
    'admin@empresa.com',
    '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5YmMxSUVaVjL6',
    'Administrador',
    'TI',
    'admin',
    true
)
ON CONFLICT (username) DO NOTHING;

-- ============================================
-- CONFIGURAÇÕES PADRÃO DO SISTEMA
-- ============================================
INSERT INTO system_config (config_key, config_value, description, data_type) VALUES
('MIN_CONFIDENCE_THRESHOLD', '0.5', 'Confiança mínima para classificação', 'decimal'),
('ENABLE_REAL_TIME_ALERTS', 'true', 'Habilitar alertas em tempo real', 'boolean'),
('EMAIL_CHECK_INTERVAL_SECONDS', '60', 'Intervalo de verificação de e-mails', 'integer'),
('DATA_RETENTION_DAYS', '90', 'Dias de retenção de dados (LGPD)', 'integer'),
('ANONYMIZE_AFTER_DAYS', '60', 'Dias para anonimizar dados', 'integer'),
('MAX_ATTACHMENT_SIZE_MB', '50', 'Tamanho máximo de anexo', 'integer'),
('ENABLE_URL_SCANNING', 'true', 'Habilitar scanning de URLs', 'boolean'),
('ML_MODEL_VERSION', '1.0', 'Versão do modelo ML em uso', 'string')
ON CONFLICT (config_key) DO NOTHING;

-- ============================================
-- DADOS DE EXEMPLO (APENAS AMBIENTE DE DESENVOLVIMENTO/TESTES)
-- ============================================
-- Os blocos abaixo populam e-mails de exemplo para permitir testar o dashboard,
-- o Power BI e os relatórios sem depender de uma caixa de e-mail real.
-- NÃO execute esta seção em produção.

DO $$
DECLARE
    v_safe_email_id INTEGER;
    v_suspicious_email_id INTEGER;
    v_phishing_email_id INTEGER;
BEGIN
    IF current_setting('phishing_detection.seed_sample_data', true) IS DISTINCT FROM 'true' THEN
        RAISE NOTICE 'Dados de exemplo não inseridos. Defina phishing_detection.seed_sample_data=true para incluí-los.';
        RETURN;
    END IF;

    INSERT INTO emails (message_id, subject, sender, recipients, received_date, email_body,
                         classification, confidence, is_validated)
    VALUES (
        'seed-safe-001',
        'Reunião de equipe amanhã',
        'joao.silva@empresa.com',
        'equipe@empresa.com',
        CURRENT_TIMESTAMP - INTERVAL '2 days',
        'Olá, confirmando nossa reunião às 10h na sala 3.',
        'safe',
        95.50,
        true
    )
    ON CONFLICT (message_id) DO NOTHING
    RETURNING id INTO v_safe_email_id;

    INSERT INTO emails (message_id, subject, sender, recipients, received_date, email_body,
                         classification, confidence, is_validated)
    VALUES (
        'seed-suspicious-001',
        'Confirme seus dados',
        'suporte@servico-externo.com',
        'financeiro@empresa.com',
        CURRENT_TIMESTAMP - INTERVAL '1 day',
        'Prezado cliente, por favor confirme seus dados de acesso clicando no link abaixo.',
        'suspicious',
        55.00,
        false
    )
    ON CONFLICT (message_id) DO NOTHING
    RETURNING id INTO v_suspicious_email_id;

    INSERT INTO emails (message_id, subject, sender, recipients, received_date, email_body,
                         classification, confidence, is_validated)
    VALUES (
        'seed-phishing-001',
        'URGENTE: Sua conta será suspensa',
        'security@bank-alert.tk',
        'financeiro@empresa.com',
        CURRENT_TIMESTAMP - INTERVAL '3 hours',
        'Clique aqui imediatamente para verificar sua conta: http://192.168.10.5/login-bank',
        'phishing',
        92.40,
        true
    )
    ON CONFLICT (message_id) DO NOTHING
    RETURNING id INTO v_phishing_email_id;

    IF v_phishing_email_id IS NOT NULL THEN
        INSERT INTO phishing_indicators (email_id, indicator_type, indicator_value, risk_score)
        VALUES
            (v_phishing_email_id, 'suspicious_url', '1 URL(s) suspeita(s) detectada(s)', 25.00),
            (v_phishing_email_id, 'urgent_language', '1 termo(s) de urgência detectado(s)', 20.00);

        INSERT INTO urls_detected (email_id, url, is_suspicious, reputation_score)
        VALUES (v_phishing_email_id, 'http://192.168.10.5/login-bank', true, 5.00);
    END IF;

    IF v_suspicious_email_id IS NOT NULL THEN
        INSERT INTO phishing_indicators (email_id, indicator_type, indicator_value, risk_score)
        VALUES (v_suspicious_email_id, 'generic_greeting', 'Saudação genérica detectada', 30.00);
    END IF;

    INSERT INTO daily_statistics (statistic_date, total_emails_processed, emails_classified_safe,
                                   emails_classified_suspicious, emails_classified_phishing,
                                   avg_processing_time_ms, model_accuracy)
    VALUES (CURRENT_DATE, 3, 1, 1, 1, 850.00, 0.9600)
    ON CONFLICT (statistic_date) DO NOTHING;

    RAISE NOTICE 'Dados de exemplo inseridos com sucesso.';
END $$;

-- ============================================
-- FIM DO SEED
-- ============================================
