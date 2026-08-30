namespace PhishingDetectionAPI.Tests.Unit.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Application.Services;
using PhishingDetectionAPI.Infrastructure.MachineLearning;
using Xunit;

public class EmailClassificationServiceTests
{
    private readonly Mock<IMlServiceClient> _mlServiceClientMock = new();

    private EmailClassificationService CreateService() =>
        new(_mlServiceClientMock.Object, NullLogger<EmailClassificationService>.Instance);

    [Fact]
    public async Task ClassifyEmailAsync_WhenMlServiceSucceeds_ReturnsMappedResponse()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyAsync(It.IsAny<MlClassifyRequest>()))
            .ReturnsAsync(new MlClassificationResult
            {
                Classification = "phishing",
                Confidence = 92.4m,
                Indicators = new List<MlIndicatorResult>
                {
                    new() { Type = "suspicious_url", Description = "URL suspeita", RiskScore = 25m }
                },
                SuspiciousUrls = new List<string> { "http://192.168.1.1/login" },
                Recommendation = "Não clique em links."
            });
        var service = CreateService();

        var result = await service.ClassifyEmailAsync(new ClassifyEmailRequest
        {
            Subject = "Urgente",
            Body = "Clique aqui",
            Sender = "attacker@malicious.tk"
        });

        Assert.Equal("phishing", result.Classification);
        Assert.Equal(92.4m, result.Confidence);
        Assert.Single(result.Indicators);
        Assert.Single(result.SuspiciousUrls);
    }

    [Fact]
    public async Task ClassifyEmailAsync_WhenMlServiceThrows_ReturnsConservativeFallback()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyAsync(It.IsAny<MlClassifyRequest>()))
            .ThrowsAsync(new HttpRequestException("ML service unavailable"));
        var service = CreateService();

        var result = await service.ClassifyEmailAsync(new ClassifyEmailRequest
        {
            Subject = "Assunto",
            Body = "Corpo",
            Sender = "someone@empresa.com"
        });

        Assert.Equal("suspicious", result.Classification);
        Assert.Equal(0m, result.Confidence);
    }

    [Fact]
    public async Task ClassifyEmailsBatchAsync_WhenMlServiceSucceeds_ReturnsAllResults()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyBatchAsync(It.IsAny<List<MlClassifyRequest>>()))
            .ReturnsAsync(new List<MlClassificationResult>
            {
                new() { Classification = "safe", Confidence = 95m },
                new() { Classification = "phishing", Confidence = 88m }
            });
        var service = CreateService();

        var results = await service.ClassifyEmailsBatchAsync(new List<ClassifyEmailRequest>
        {
            new() { Subject = "A", Body = "A", Sender = "a@empresa.com" },
            new() { Subject = "B", Body = "B", Sender = "b@empresa.com" }
        });

        Assert.Equal(2, results.Count);
        Assert.Equal("safe", results[0].Classification);
        Assert.Equal("phishing", results[1].Classification);
    }
}
