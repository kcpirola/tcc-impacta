namespace PhishingDetectionAPI.Infrastructure.Security;

/// <summary>
/// Configurações de JWT (JSON Web Token).
/// Carregadas do arquivo appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>Chave secreta para assinar tokens (deve ter pelo menos 32 caracteres).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Número de minutos até o token expirar.</summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>Emissor do token (identificador da aplicação).</summary>
    public string Issuer { get; set; } = "PhishingDetectionAPI";

    /// <summary>Audiência do token (quem pode usar o token).</summary>
    public string Audience { get; set; } = "PhishingDetectionClients";

    /// <summary>Valida se as configurações obrigatórias estão preenchidas.</summary>
    /// <returns>True se válido, False caso contrário.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) && 
               SecretKey.Length >= 32 &&
               ExpirationMinutes > 0;
    }
}
