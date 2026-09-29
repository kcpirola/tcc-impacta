namespace PhishingDetectionAPI.Tests.Unit.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Application.Services;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using PhishingDetectionAPI.Infrastructure.MachineLearning;
using Xunit;

public class EmailClassificationServiceTests
{
    private readonly Mock<IMlServiceClient> _mlServiceClientMock = new();
    private readonly Mock<IEmailRepository> _emailRepositoryMock = new();

    private EmailClassificationService CreateService() =>
        new(_mlServiceClientMock.Object, _emailRepositoryMock.Object,
            NullLogger<EmailClassificationService>.Instance);

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
    public async Task ClassifyEmailAsync_SavesEmailWithClassificationIndicatorsAndUrls()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyAsync(It.IsAny<MlClassifyRequest>()))
            .ReturnsAsync(new MlClassificationResult
            {
                Classification = "phishing",
                Confidence = 92.4m,
                Indicators = new List<MlIndicatorResult>
                {
                    new() { Type = "suspicious_url", Description = "1 URL \"suspeita\" detectada", RiskScore = 25m }
                },
                SuspiciousUrls = new List<string> { "http://portal-falso.xyz/login" },
                Recommendation = "Não clique em links."
            });
        Email? gravado = null;
        _emailRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Email>())).Callback<Email>(e => gravado = e);
        var service = CreateService();

        await service.ClassifyEmailAsync(new ClassifyEmailRequest
        {
            Subject = "Urgente",
            Body = "Clique aqui",
            Sender = "attacker@malicious.tk"
        });

        Assert.NotNull(gravado);
        Assert.Equal(EmailClassification.Phishing, gravado!.Classification);
        Assert.Equal(92.4m, gravado.Confidence);
        Assert.Equal("Urgente", gravado.Subject);
        Assert.Equal("attacker@malicious.tk", gravado.Sender);
        Assert.False(string.IsNullOrWhiteSpace(gravado.MessageId));
        Assert.Single(gravado.Indicators);
        Assert.Single(gravado.UrlsDetected);
        Assert.True(gravado.UrlsDetected.First().IsSuspicious);
    }

    [Fact]
    public async Task ClassifyEmailAsync_WhenMlServiceThrows_DoesNotSaveEmail()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyAsync(It.IsAny<MlClassifyRequest>()))
            .ThrowsAsync(new HttpRequestException("ML service unavailable"));
        var service = CreateService();

        await service.ClassifyEmailAsync(new ClassifyEmailRequest
        {
            Subject = "Assunto",
            Body = "Corpo",
            Sender = "someone@empresa.com"
        });

        // O fallback é um palpite conservador, não uma análise: não deve sujar o dashboard.
        _emailRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Email>()), Times.Never);
    }

    [Fact]
    public async Task ClassifyEmailAsync_WhenSavingFails_StillReturnsClassification()
    {
        _mlServiceClientMock
            .Setup(c => c.ClassifyAsync(It.IsAny<MlClassifyRequest>()))
            .ReturnsAsync(new MlClassificationResult { Classification = "safe", Confidence = 97m });
        _emailRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Email>()))
            .ThrowsAsync(new InvalidOperationException("banco indisponível"));
        var service = CreateService();

        var result = await service.ClassifyEmailAsync(new ClassifyEmailRequest
        {
            Subject = "Assunto",
            Body = "Corpo",
            Sender = "colega@empresa.com"
        });

        Assert.Equal("safe", result.Classification);
        Assert.Equal(97m, result.Confidence);
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
