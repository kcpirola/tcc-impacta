namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Representa um log de auditoria de ações do usuário.
/// Necessário para conformidade com LGPD e rastreabilidade.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>ID do usuário que realizou a ação (pode ser nulo para ações do sistema).</summary>
    public int? UserId { get; set; }

    /// <summary>Tipo de ação realizada (login, view_email, export_report, etc).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Tipo de recurso acessado (email, report, user, etc).</summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>ID do recurso acessado.</summary>
    public int? ResourceId { get; set; }

    /// <summary>Descrição detalhada da ação.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Endereço IP do cliente.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>User-Agent do cliente.</summary>
    public string? UserAgent { get; set; }

    // Relacionamento
    public virtual User? User { get; set; }
}
