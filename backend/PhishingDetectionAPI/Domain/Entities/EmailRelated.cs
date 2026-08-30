namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Representa um indicador de phishing detectado em um e-mail.
/// Exemplos: suspicious_link, urgent_language, unknown_sender, etc.
/// </summary>
public class PhishingIndicator : BaseEntity
{
    /// <summary>ID do e-mail ao qual este indicador pertence.</summary>
    public int EmailId { get; set; }

    /// <summary>Tipo de indicador detectado.</summary>
    public string IndicatorType { get; set; } = string.Empty;

    /// <summary>Valor ou descrição do indicador.</summary>
    public string IndicatorValue { get; set; } = string.Empty;

    /// <summary>Score de risco associado a este indicador (0-100).</summary>
    public decimal RiskScore { get; set; } = 0m;

    // Relacionamento
    public virtual Email? Email { get; set; }
}

/// <summary>
/// Representa uma URL detectada em um e-mail.
/// Armazena informações sobre validação de URL.
/// </summary>
public class UrlDetected : BaseEntity
{
    /// <summary>ID do e-mail ao qual esta URL pertence.</summary>
    public int EmailId { get; set; }

    /// <summary>A URL detectada.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Indica se a URL é suspeita.</summary>
    public bool IsSuspicious { get; set; } = false;

    /// <summary>Score de reputação da URL (0-100).</summary>
    public decimal ReputationScore { get; set; } = 50m;

    /// <summary>Data e hora da verificação da URL.</summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    // Relacionamento
    public virtual Email? Email { get; set; }
}

/// <summary>
/// Representa um anexo de um e-mail.
/// Armazena informações sobre validação de malware.
/// </summary>
public class Attachment : BaseEntity
{
    /// <summary>ID do e-mail ao qual este anexo pertence.</summary>
    public int EmailId { get; set; }

    /// <summary>Nome do arquivo anexado.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Tamanho do arquivo em bytes.</summary>
    public long FileSizeBytes { get; set; } = 0;

    /// <summary>Hash do arquivo (SHA256).</summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>Indica se o arquivo é malicioso.</summary>
    public bool IsMalicious { get; set; } = false;

    /// <summary>Resultado da análise de malware.</summary>
    public string ScanResult { get; set; } = string.Empty;

    /// <summary>Data e hora da análise do arquivo.</summary>
    public DateTime? ScannedAt { get; set; }

    // Relacionamento
    public virtual Email? Email { get; set; }
}
