namespace PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Registra uma medição de desempenho de um modelo de Machine Learning (Naive Bayes, Random Forest).
/// </summary>
public class ModelPerformance : BaseEntity
{
    /// <summary>Nome do modelo avaliado (naive_bayes, random_forest, ensemble).</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Versão do modelo avaliado.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Acurácia do modelo (0-1).</summary>
    public decimal Accuracy { get; set; }

    /// <summary>Precisão do modelo (0-1).</summary>
    public decimal Precision { get; set; }

    /// <summary>Recall do modelo (0-1).</summary>
    public decimal Recall { get; set; }

    /// <summary>F1-Score do modelo (0-1).</summary>
    public decimal F1Score { get; set; }

    /// <summary>Quantidade de e-mails usados no teste.</summary>
    public int TestedEmails { get; set; }

    /// <summary>Data em que o teste foi realizado.</summary>
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Observações sobre o teste.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Agrega estatísticas diárias de processamento de e-mails para relatórios e Power BI.
/// </summary>
public class DailyStatistics : BaseEntity
{
    /// <summary>Data de referência das estatísticas (única por dia).</summary>
    public DateOnly StatisticDate { get; set; }

    /// <summary>Total de e-mails processados no dia.</summary>
    public int TotalEmailsProcessed { get; set; } = 0;

    /// <summary>Total de e-mails classificados como seguros.</summary>
    public int EmailsClassifiedSafe { get; set; } = 0;

    /// <summary>Total de e-mails classificados como suspeitos.</summary>
    public int EmailsClassifiedSuspicious { get; set; } = 0;

    /// <summary>Total de e-mails classificados como phishing.</summary>
    public int EmailsClassifiedPhishing { get; set; } = 0;

    /// <summary>Tempo médio de processamento por e-mail, em milissegundos.</summary>
    public decimal AvgProcessingTimeMs { get; set; }

    /// <summary>Acurácia média do modelo no dia.</summary>
    public decimal ModelAccuracy { get; set; }
}
