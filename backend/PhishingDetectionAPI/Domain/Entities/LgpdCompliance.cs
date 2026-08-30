namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Registra o consentimento do usuário para tratamento de dados, conforme a LGPD.
/// </summary>
public class LgpdConsent : BaseEntity
{
    /// <summary>ID do usuário que forneceu o consentimento.</summary>
    public int UserId { get; set; }

    /// <summary>Tipo de consentimento (data_processing, email_analysis, etc).</summary>
    public string ConsentType { get; set; } = string.Empty;

    /// <summary>Indica se o usuário consentiu.</summary>
    public bool IsConsented { get; set; } = false;

    /// <summary>Data em que o consentimento foi registrado.</summary>
    public DateTime? ConsentDate { get; set; }

    /// <summary>Endereço IP do cliente no momento do consentimento.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>User-Agent do cliente no momento do consentimento.</summary>
    public string? UserAgent { get; set; }

    // Relacionamento
    public virtual User? User { get; set; }
}

/// <summary>
/// Representa uma solicitação de exclusão de dados (direito ao esquecimento, LGPD).
/// </summary>
public class DataDeletionRequest : BaseEntity
{
    /// <summary>ID do usuário solicitante (pode ser nulo para solicitações externas).</summary>
    public int? UserId { get; set; }

    /// <summary>Endereço de e-mail alvo da solicitação de exclusão.</summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>Motivo da solicitação.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Status da solicitação.</summary>
    public DataDeletionStatus Status { get; set; } = DataDeletionStatus.Pending;

    /// <summary>Data em que a solicitação foi feita.</summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Data em que a solicitação foi concluída.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Observações sobre o processamento da solicitação.</summary>
    public string? Notes { get; set; }

    // Relacionamento
    public virtual User? User { get; set; }
}

/// <summary>Define os status possíveis de uma solicitação de exclusão de dados.</summary>
public enum DataDeletionStatus
{
    Pending = 0,
    Approved = 1,
    Completed = 2,
    Rejected = 3
}
