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

    private void SeedEmail(string messageId, EmailClassification classification, UserFeedback? feedback = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PhishingDetectionContext>();
        db.Emails.Add(new Email
        {
            MessageId = messageId,
            Subject = $"Assunto {messageId}",
            Sender = "sender@empresa.com",
            Recipients = "destinatario@empresa.com",
            EmailBody = "Corpo do e-mail",
            Classification = classification,
            Confidence = 90m,
            UserFeedback = feedback
        });
        db.SaveChanges();
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
        SeedEmail("msg-safe-1", EmailClassification.Safe);
        SeedEmail("msg-safe-2", EmailClassification.Safe);

        var token = await GetAuthTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/emails/by-classification/safe");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var emails = body.EnumerateArray().ToList();
        Assert.Equal(2, emails.Count);
        Assert.All(emails, e => Assert.Equal("safe", e.GetProperty("classification").GetString()));
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
