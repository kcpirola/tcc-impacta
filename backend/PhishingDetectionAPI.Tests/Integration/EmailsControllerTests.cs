namespace PhishingDetectionAPI.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Infrastructure.Data;
using Xunit;

public class EmailsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmailsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private Email SeedEmail(string messageId, EmailClassification classification, UserFeedback? feedback = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PhishingDetectionContext>();
        var email = new Email
        {
            MessageId = messageId,
            Subject = $"Assunto {messageId}",
            Sender = "sender@empresa.com",
            Recipients = "destinatario@empresa.com",
            EmailBody = "Corpo do e-mail",
            Classification = classification,
            Confidence = 90m,
            UserFeedback = feedback
        };
        db.Emails.Add(email);
        db.SaveChanges();
        return email;
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = CustomWebApplicationFactory.TestUsername,
            password = CustomWebApplicationFactory.TestPassword
        });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private async Task<JsonElement> ChamarAsync(HttpMethod metodo, string url, object? corpo = null)
    {
        using var requisicao = new HttpRequestMessage(metodo, url)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync()) }
        };
        if (corpo != null) requisicao.Content = JsonContent.Create(corpo);

        var resposta = await _client.SendAsync(requisicao);
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Classify_SavesEmail_SoStatisticsAndListingSeeIt()
    {
        var antes = (await ChamarAsync(HttpMethod.Get, "/api/emails/statistics")).GetProperty("safe").GetInt32();
        var assunto = $"Ata da reunião {Guid.NewGuid():N}";

        await ChamarAsync(HttpMethod.Post, "/api/emails/classify", new
        {
            subject = assunto,
            body = "Segue a ata da reunião de ontem, com os próximos passos.",
            sender = "colega@empresa.com"
        });

        var depois = (await ChamarAsync(HttpMethod.Get, "/api/emails/statistics")).GetProperty("safe").GetInt32();
        Assert.Equal(antes + 1, depois);

        var listagem = await ChamarAsync(HttpMethod.Get, "/api/emails/by-classification/safe?skip=0&take=100");
        var assuntos = listagem.EnumerateArray().Select(e => e.GetProperty("subject").GetString()).ToList();
        Assert.Contains(assunto, assuntos);
    }

    [Fact]
    public async Task Statistics_WithoutToken_ReturnsUnauthorized()
    {
        var resposta = await _client.GetAsync("/api/emails/statistics");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Classify_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/emails/classify", new
        {
            subject = "Assunto",
            body = "Corpo do e-mail",
            sender = "sender@empresa.com"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Classify_WithValidToken_ReturnsClassificationFromMlStub()
    {
        var token = await GetAuthTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/emails/classify")
        {
            Content = JsonContent.Create(new
            {
                subject = "Reunião amanhã",
                body = "Confirmando nosso encontro às 10h.",
                sender = "colega@empresa.com"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("safe", body.GetProperty("classification").GetString());
    }

    [Fact]
    public async Task Classify_WithMissingRequiredFields_ReturnsBadRequest()
    {
        var token = await GetAuthTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/emails/classify")
        {
            Content = JsonContent.Create(new { subject = "", body = "", sender = "" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByClassification_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/emails/by-classification/safe");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByClassification_WithInvalidClassification_ReturnsBadRequest()
    {
        var token = await GetAuthTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/emails/by-classification/nao-existe");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByClassification_WithValidToken_ReturnsOnlyMatchingEmails()
    {
        SeedEmail("msg-phishing-1", EmailClassification.Phishing, UserFeedback.Correct);
        var assuntoSeguro1 = SeedEmail("msg-safe-1", EmailClassification.Safe).Subject;
        var assuntoSeguro2 = SeedEmail("msg-safe-2", EmailClassification.Safe).Subject;

        var token = await GetAuthTokenAsync();
        // take alto: outros testes desta classe também gravam e-mails seguros ao classificar
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/emails/by-classification/safe?take=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var emails = body.EnumerateArray().ToList();
        Assert.All(emails, e => Assert.Equal("safe", e.GetProperty("classification").GetString()));
        var assuntos = emails.Select(e => e.GetProperty("subject").GetString()).ToList();
        Assert.Contains(assuntoSeguro1, assuntos);
        Assert.Contains(assuntoSeguro2, assuntos);
    }

    [Fact]
    public async Task GetByClassification_MapsUserFeedbackToLowercaseString()
    {
        SeedEmail("msg-phishing-feedback", EmailClassification.Phishing, UserFeedback.Incorrect);

        var token = await GetAuthTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/emails/by-classification/phishing");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var email = body.EnumerateArray().Single(e => e.GetProperty("subject").GetString() == "Assunto msg-phishing-feedback");
        Assert.Equal("incorrect", email.GetProperty("userFeedback").GetString());
    }
}
