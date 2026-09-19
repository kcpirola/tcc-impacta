namespace PhishingDetectionAPI.Application.Dtos;

using System.ComponentModel.DataAnnotations;

/// <summary>DTO para autocadastro: o próprio usuário cria a conta e escolhe a senha.</summary>
/// <remarks>Não há campo de papel de propósito: todo autocadastro entra como "user".</remarks>
public class CreateUserRequest
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

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
