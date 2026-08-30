namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Representa uma configuração chave-valor do sistema (thresholds, retenção, etc).
/// </summary>
public class SystemConfig : BaseEntity
{
    /// <summary>Chave única da configuração.</summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>Valor da configuração, armazenado como texto.</summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>Descrição do propósito da configuração.</summary>
    public string? Description { get; set; }

    /// <summary>Tipo de dado do valor (string, integer, boolean, decimal).</summary>
    public string DataType { get; set; } = "string";
}

/// <summary>
/// Representa uma notificação enviada a um usuário (alerta em tempo real, relatório pronto, etc).
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>ID do usuário destinatário da notificação.</summary>
    public int UserId { get; set; }

    /// <summary>ID do e-mail relacionado à notificação (se houver).</summary>
    public int? EmailId { get; set; }

    /// <summary>Tipo da notificação (phishing_detected, report_ready, etc).</summary>
    public string NotificationType { get; set; } = string.Empty;

    /// <summary>Título da notificação.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Mensagem detalhada da notificação.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Indica se a notificação já foi lida.</summary>
    public bool IsRead { get; set; } = false;

    /// <summary>Data em que a notificação foi lida.</summary>
    public DateTime? ReadAt { get; set; }

    // Relacionamentos
    public virtual User? User { get; set; }
    public virtual Email? Email { get; set; }
}
