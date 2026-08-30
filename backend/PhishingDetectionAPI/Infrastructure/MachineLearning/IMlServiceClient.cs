namespace PhishingDetectionAPI.Infrastructure.MachineLearning;

using Refit;

/// <summary>
/// Cliente Refit para o microserviço Python de Machine Learning.
/// Abstrai a comunicação HTTP, seguindo o princípio de Inversão de Dependência (DIP).
/// </summary>
public interface IMlServiceClient
{
    /// <summary>Verifica a saúde do microserviço de ML.</summary>
    [Get("/health")]
    Task<ApiResponse<object>> GetHealthAsync();

    /// <summary>Classifica um único e-mail.</summary>
    /// <param name="request">Dados do e-mail no formato esperado pelo serviço Python.</param>
    [Post("/classify")]
    Task<MlClassificationResult> ClassifyAsync([Body] MlClassifyRequest request);

    /// <summary>Classifica múltiplos e-mails em lote.</summary>
    /// <param name="requests">Lista de e-mails a classificar.</param>
    [Post("/classify-batch")]
    Task<List<MlClassificationResult>> ClassifyBatchAsync([Body] List<MlClassifyRequest> requests);
}

/// <summary>Payload enviado ao microserviço de ML.</summary>
public class MlClassifyRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string? Recipients { get; set; }
}

/// <summary>Resposta recebida do microserviço de ML.</summary>
public class MlClassificationResult
{
    public string Classification { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public List<MlIndicatorResult> Indicators { get; set; } = new();
    public List<string> SuspiciousUrls { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>Indicador de phishing retornado pelo microserviço de ML.</summary>
public class MlIndicatorResult
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
}
