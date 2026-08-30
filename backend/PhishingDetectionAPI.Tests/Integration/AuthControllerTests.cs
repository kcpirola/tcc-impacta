namespace PhishingDetectionAPI.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = CustomWebApplicationFactory.TestUsername,
            password = CustomWebApplicationFactory.TestPassword
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "does-not-exist",
            password = "whatever"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUsername_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "",
            password = "whatever"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
