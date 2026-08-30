namespace PhishingDetectionAPI.Application.Dtos;

/// <summary>
/// DTO para requisição de login.
/// </summary>
public class LoginRequest
{
    /// <summary>Nome de usuário ou e-mail.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Senha do usuário.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de login/autenticação.
/// </summary>
public class AuthResponse
{
    /// <summary>Token JWT para autenticação.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Tipo do token (geralmente "Bearer").</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Segundos até o token expirar.</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Informações do usuário autenticado.</summary>
    public UserDto? User { get; set; }
}

/// <summary>
/// DTO para informações do usuário.
/// </summary>
public class UserDto
{
    /// <summary>ID do usuário.</summary>
    public int Id { get; set; }

    /// <summary>Nome de usuário.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Email do usuário.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Nome completo.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Departamento.</summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>Papel do usuário.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Indica se está ativo.</summary>
    public bool IsActive { get; set; }

    /// <summary>Último login.</summary>
    public DateTime? LastLogin { get; set; }
}
