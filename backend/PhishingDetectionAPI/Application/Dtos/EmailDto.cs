namespace PhishingDetectionAPI.Application.Dtos;

/// <summary>
/// DTO para requisição de classificação de e-mail.
/// Enviado pelo cliente ao backend.
/// </summary>
public class ClassifyEmailRequest
{
    /// <summary>Assunto do e-mail.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Corpo do e-mail.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Endereço do remetente.</summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>Destinatários (separados por ;).</summary>
    public string Recipients { get; set; } = string.Empty;

    /// <summary>Headers do e-mail em JSON (opcional).</summary>
    public string? Headers { get; set; }
}

/// <summary>
/// DTO para resposta de classificação de e-mail.
/// Retornado pelo backend ao cliente.
/// </summary>
public class ClassificationResponse
{
    /// <summary>Classificação: "safe", "suspicious", ou "phishing".</summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>Nível de confiança (0-100).</summary>
    public decimal Confidence { get; set; }

    /// <summary>Indicadores detectados.</summary>
    public List<IndicatorDto> Indicators { get; set; } = new();

    /// <summary>URLs suspeitas detectadas.</summary>
    public List<string> SuspiciousUrls { get; set; } = new();

    /// <summary>Mensagem recomendação ao usuário.</summary>
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// DTO para um indicador de phishing.
/// </summary>
public class IndicatorDto
{
    /// <summary>Tipo do indicador.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Descrição do indicador.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Score de risco (0-100).</summary>
    public decimal RiskScore { get; set; }
}

/// <summary>
/// DTO para listagem de e-mails processados.
/// </summary>
public class EmailDto
{
    /// <summary>ID do e-mail no sistema.</summary>
    public int Id { get; set; }

    /// <summary>Assunto do e-mail.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Remetente.</summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>Classificação.</summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>Confiança.</summary>
    public decimal Confidence { get; set; }

    /// <summary>Data de recebimento.</summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>Data de processamento.</summary>
    public DateTime ProcessedDate { get; set; }

    /// <summary>Se foi validado pelo usuário.</summary>
    public bool IsValidated { get; set; }

    /// <summary>Feedback do usuário (se houver).</summary>
    public string? UserFeedback { get; set; }
}

/// <summary>
/// DTO para enviar feedback sobre classificação.
/// </summary>
/// <summary>Totais de e-mails por classificação, para os cartões do dashboard.</summary>
public class EmailStatisticsDto
{
    public int Safe { get; set; }

    public int Suspicious { get; set; }

    public int Phishing { get; set; }

    public int Total => Safe + Suspicious + Phishing;
}

public class EmailFeedbackRequest
{
    /// <summary>ID do e-mail.</summary>
    public int EmailId { get; set; }

    /// <summary>Feedback: "correct", "incorrect", ou "uncertain".</summary>
    public string Feedback { get; set; } = string.Empty;
}
