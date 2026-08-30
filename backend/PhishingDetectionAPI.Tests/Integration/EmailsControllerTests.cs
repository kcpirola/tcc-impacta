namespace PhishingDetectionAPI.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class EmailsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmailsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
}
