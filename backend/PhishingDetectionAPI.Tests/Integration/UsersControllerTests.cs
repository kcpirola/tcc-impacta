namespace PhishingDetectionAPI.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object NovoUsuario(string username, string password = "senha-escolhida-pela-ana") => new
    {
        username,
        email = $"{username}@empresa.com",
        fullName = "Ana Teste",
        department = "Financeiro",
        password
    };

    [Fact]
    public async Task Register_WithoutToken_CreatesUserThatCanLoginWithChosenPassword()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", NovoUsuario("ana.cadastro"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { username = "ana.cadastro", password = "senha-escolhida-pela-ana" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Register_TryingToSetAdminRole_CreatesRegularUser()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", new
        {
            username = "ana.esperta",
            email = "ana.esperta@empresa.com",
            fullName = "Ana Esperta",
            password = "senha-escolhida-pela-ana",
            role = "admin"
        });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("User", corpo.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsConflict()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", NovoUsuario(CustomWebApplicationFactory.TestUsername));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Register_WithPasswordBelowMinimum_ReturnsBadRequest()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", NovoUsuario("ana.curta", "12345"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Register_WithSixCharacterPassword_IsAccepted()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", NovoUsuario("ana.seis", "123456"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        var resposta = await _client.PostAsJsonAsync("/api/users", new
        {
            username = "ana.email",
            email = "nao-e-email",
            fullName = "Ana",
            password = "senha-escolhida-pela-ana"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
