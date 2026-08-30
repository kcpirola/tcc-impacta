namespace PhishingDetectionAPI.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using PhishingDetectionAPI.Infrastructure.Security;

/// <summary>
/// Serviço de autenticação e geração de tokens JWT.
/// Implementa o padrão Service e o princípio de Responsabilidade Única (SRP) do SOLID.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    /// <summary>Inicializa uma nova instância de AuthenticationService.</summary>
    public AuthenticationService(IUserRepository userRepository, JwtSettings jwtSettings)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
    }

    /// <summary>Autentica um usuário e gera token JWT.</summary>
    public async Task<AuthResponse> AuthenticateAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Nome de usuário e senha são obrigatórios");
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username);
        
        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException("Usuário não encontrado ou inativo");
        }

        // Verificar senha (usando BCrypt em produção)
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Senha incorreta");
        }

        // Gerar token JWT
        var token = GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Department = user.Department,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                LastLogin = user.LastLogin
            }
        };
    }

    /// <summary>Valida um token JWT.</summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return validatedToken is JwtSecurityToken;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Obtém o ID do usuário de um token JWT.</summary>
    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            // JwtSecurityTokenHandler serializa ClaimTypes.NameIdentifier como "nameid" (nome curto do JWT);
            // ReadToken (ao contrário do pipeline de autenticação) não remapeia de volta para a URI longa.
            var userIdClaim = jwtToken?.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Gera um novo token JWT para o usuário.</summary>
    private string GenerateToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>Verifica se a senha coincide com o hash armazenado.</summary>
    /// <remarks>
    /// Em produção, use BCrypt.Net ou similar para verificar hashes.
    /// Este é apenas um exemplo simples.
    /// </remarks>
    private bool VerifyPassword(string password, string hash)
    {
        // TODO: Implementar verificação com BCrypt
        // return BCrypt.Net.BCrypt.Verify(password, hash);
        
        // Placeholder para exemplo
        return !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(hash);
    }
}
