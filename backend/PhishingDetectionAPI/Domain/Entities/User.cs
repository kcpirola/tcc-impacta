namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Representa um usuário do sistema.
/// Implementa o padrão de entidade com ID e timestamps.
/// </summary>
public class User : BaseEntity
{
    /// <summary>Nome de usuário único do sistema.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Endereço de e-mail único do usuário.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash da senha (nunca armazenar em texto plano).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Nome completo do usuário.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Departamento do usuário na empresa.</summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>Papel do usuário (admin, analyst, user).</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>Indica se o usuário está ativo.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Data do último login.</summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>Consentimento LGPD para processamento de dados.</summary>
    public bool HasLgpdConsent { get; set; } = false;

    // Relacionamento
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<LgpdConsent> LgpdConsents { get; set; } = new List<LgpdConsent>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

/// <summary>
/// Define os papéis possíveis de um usuário no sistema.
/// </summary>
public enum UserRole
{
    /// <summary>Usuário comum (apenas visualiza e fornece feedback).</summary>
    User = 0,

    /// <summary>Analista de segurança (acesso a relatórios e análises).</summary>
    Analyst = 1,

    /// <summary>Administrador do sistema (acesso total).</summary>
    Admin = 2
}
