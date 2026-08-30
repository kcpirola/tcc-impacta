namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Representa um e-mail processado pelo sistema.
/// Armazena informações de classificação e análise.
/// </summary>
public class Email : BaseEntity
{
    /// <summary>ID único do e-mail fornecido pelo servidor.</summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>Assunto do e-mail.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Endereço do remetente.</summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>Lista de destinatários (separados por ;).</summary>
    public string Recipients { get; set; } = string.Empty;

    /// <summary>Data de recebimento do e-mail.</summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>Data de processamento pelo sistema.</summary>
    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Corpo do e-mail (pode ser truncado).</summary>
    public string EmailBody { get; set; } = string.Empty;

    /// <summary>Headers do e-mail em formato JSON.</summary>
    public string EmailHeaders { get; set; } = string.Empty;

    /// <summary>Classificação do e-mail: safe, suspicious, ou phishing.</summary>
    public EmailClassification Classification { get; set; } = EmailClassification.Safe;

    /// <summary>Confiança da classificação (0-100).</summary>
    public decimal Confidence { get; set; } = 0m;

    /// <summary>Indica se o usuário já validou esta classificação.</summary>
    public bool IsValidated { get; set; } = false;

    /// <summary>Feedback do usuário sobre a classificação.</summary>
    public UserFeedback? UserFeedback { get; set; }

    // Relacionamentos
    public virtual ICollection<PhishingIndicator> Indicators { get; set; } = new List<PhishingIndicator>();
    public virtual ICollection<UrlDetected> UrlsDetected { get; set; } = new List<UrlDetected>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}

/// <summary>
/// Define as classificações possíveis de um e-mail.
/// </summary>
public enum EmailClassification
{
    /// <summary>E-mail seguro (não é phishing).</summary>
    Safe = 0,

    /// <summary>E-mail suspeito (requer atenção).</summary>
    Suspicious = 1,

    /// <summary>E-mail identificado como phishing.</summary>
    Phishing = 2
}

/// <summary>
/// Define os tipos de feedback que um usuário pode fornecer.
/// </summary>
public enum UserFeedback
{
    /// <summary>Usuário concorda com a classificação.</summary>
    Correct = 0,

    /// <summary>Usuário discorda da classificação.</summary>
    Incorrect = 1,

    /// <summary>Usuário tem dúvida sobre a classificação.</summary>
    Uncertain = 2
}
