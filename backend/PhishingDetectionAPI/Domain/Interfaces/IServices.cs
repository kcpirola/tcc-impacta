namespace PhishingDetectionAPI.Domain.Interfaces;

using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;

/// <summary>
/// Interface para serviço de autenticação.
/// Responsável por autenticar usuários e gerenciar tokens JWT.
/// Implementa o princípio de Segregação de Interface (ISP) do SOLID.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>Autentica um usuário e retorna um token JWT.</summary>
    /// <param name="request">Requisição com credenciais do usuário.</param>
    /// <returns>Resposta com token JWT se bem-sucedido.</returns>
    Task<AuthResponse> AuthenticateAsync(LoginRequest request);

    /// <summary>Valida um token JWT.</summary>
    /// <param name="token">Token JWT a validar.</param>
    /// <returns>True se o token é válido, False caso contrário.</returns>
    bool ValidateToken(string token);

    /// <summary>Obtém o ID do usuário de um token JWT.</summary>
    /// <param name="token">Token JWT.</param>
    /// <returns>ID do usuário contido no token.</returns>
    int? GetUserIdFromToken(string token);
}

/// <summary>
/// Interface para serviço de classificação de e-mail.
/// Responsável por classificar e-mails usando ML ou regras.
/// </summary>
public interface IEmailClassificationService
{
    /// <summary>Classifica um e-mail.</summary>
    /// <param name="request">Dados do e-mail a classificar.</param>
    /// <returns>Resultado da classificação.</returns>
    Task<ClassificationResponse> ClassifyEmailAsync(ClassifyEmailRequest request);

    /// <summary>Classifica múltiplos e-mails em lote.</summary>
    /// <param name="requests">Lista de e-mails a classificar.</param>
    /// <returns>Lista de resultados de classificação.</returns>
    Task<List<ClassificationResponse>> ClassifyEmailsBatchAsync(List<ClassifyEmailRequest> requests);
}

/// <summary>
/// Interface para serviço de integração com e-mail.
/// Responsável por se conectar ao servidor IMAP e recuperar e-mails.
/// </summary>
public interface IEmailIntegrationService
{
    /// <summary>Conecta ao servidor IMAP e recupera e-mails da caixa de entrada.</summary>
    /// <param name="limit">Limite de e-mails a recuperar.</param>
    /// <returns>Lista de e-mails recuperados.</returns>
    Task<List<ClassifyEmailRequest>> FetchIncomingEmailsAsync(int limit = 100);

    /// <summary>Testa a conexão com o servidor IMAP.</summary>
    /// <returns>True se a conexão foi bem-sucedida, False caso contrário.</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>Envia um e-mail de notificação.</summary>
    /// <param name="to">Destinatário.</param>
    /// <param name="subject">Assunto.</param>
    /// <param name="body">Corpo da mensagem.</param>
    /// <returns>True se enviado com sucesso, False caso contrário.</returns>
    Task<bool> SendNotificationEmailAsync(string to, string subject, string body);
}

/// <summary>
/// Interface para serviço de gestão de usuários.
/// Responsável por criar, atualizar e listar usuários.
/// </summary>
public interface IUserService
{
    /// <summary>Registra um novo usuário no sistema.</summary>
    /// <param name="username">Nome de usuário.</param>
    /// <param name="email">E-mail do usuário.</param>
    /// <param name="password">Senha do usuário.</param>
    /// <param name="fullName">Nome completo.</param>
    /// <param name="department">Departamento (opcional).</param>
    /// <param name="role">Papel do usuário no sistema.</param>
    /// <returns>DTO do usuário criado.</returns>
    Task<UserDto> RegisterUserAsync(string username, string email, string password, string fullName,
        string department = "", UserRole role = UserRole.User);

    /// <summary>Obtém um usuário pelo ID.</summary>
    /// <param name="userId">ID do usuário.</param>
    /// <returns>DTO do usuário ou null se não encontrado.</returns>
    Task<UserDto?> GetUserAsync(int userId);

    /// <summary>Atualiza dados do usuário.</summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="fullName">Novo nome completo (opcional).</param>
    /// <param name="department">Novo departamento (opcional).</param>
    /// <returns>DTO do usuário atualizado.</returns>
    Task<UserDto> UpdateUserAsync(int userId, string? fullName = null, string? department = null);

    /// <summary>Lista todos os usuários (admin).</summary>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Lista de usuários.</returns>
    Task<List<UserDto>> ListUsersAsync(int skip = 0, int take = 10);

    /// <summary>Desativa um usuário.</summary>
    /// <param name="userId">ID do usuário a desativar.</param>
    Task DeactivateUserAsync(int userId);
}

/// <summary>
/// Interface para auditoria.
/// Responsável por registrar todas as ações do usuário no sistema.
/// </summary>
public interface IAuditService
{
    /// <summary>Registra uma ação de auditoria.</summary>
    /// <param name="userId">ID do usuário (pode ser nulo para ações do sistema).</param>
    /// <param name="action">Tipo de ação.</param>
    /// <param name="resourceType">Tipo de recurso acessado.</param>
    /// <param name="resourceId">ID do recurso acessado.</param>
    /// <param name="description">Descrição da ação.</param>
    /// <param name="ipAddress">Endereço IP do cliente.</param>
    /// <param name="userAgent">User-Agent do cliente (opcional).</param>
    Task LogActionAsync(int? userId, string action, string resourceType, int? resourceId, 
        string description, string ipAddress, string? userAgent = null);

    /// <summary>Obtém logs de auditoria de um usuário.</summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="skip">Número de registros a pular.</param>
    /// <param name="take">Número de registros a retornar.</param>
    /// <returns>Lista de logs de auditoria.</returns>
    Task<List<AuditLogDto>> GetUserAuditLogsAsync(int userId, int skip = 0, int take = 20);
}

/// <summary>
/// DTO para log de auditoria (usado na interface).
/// </summary>
public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int? ResourceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
