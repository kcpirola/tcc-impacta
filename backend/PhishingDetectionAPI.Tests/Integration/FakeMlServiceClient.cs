namespace PhishingDetectionAPI.Tests.Integration;

using PhishingDetectionAPI.Infrastructure.MachineLearning;
using Refit;

/// <summary>
/// Substitui o cliente real do microserviço de ML nos testes de integração,
/// evitando dependência de rede/serviço Python em execução.
/// </summary>
public class FakeMlServiceClient : IMlServiceClient
{
    public Task<ApiResponse<object>> GetHealthAsync() =>
        throw new NotSupportedException("Não utilizado nos testes de integração.");

    public Task<MlClassificationResult> ClassifyAsync(MlClassifyRequest request)
    {
        return Task.FromResult(new MlClassificationResult
        {
            Classification = "safe",
            Confidence = 99.9m,
            Indicators = new List<MlIndicatorResult>(),
            SuspiciousUrls = new List<string>(),
            Recommendation = "E-mail seguro (stub de teste de integração)."
        });
    }

    public Task<List<MlClassificationResult>> ClassifyBatchAsync(List<MlClassifyRequest> requests)
    {
        var results = requests.Select(_ => new MlClassificationResult
        {
            Classification = "safe",
            Confidence = 99.9m,
            Indicators = new List<MlIndicatorResult>(),
            SuspiciousUrls = new List<string>(),
            Recommendation = "E-mail seguro (stub de teste de integração)."
        }).ToList();

        return Task.FromResult(results);
    }
}
