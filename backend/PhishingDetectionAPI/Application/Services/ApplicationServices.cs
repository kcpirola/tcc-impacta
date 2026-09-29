namespace PhishingDetectionAPI.Application.Services;

using AutoMapper;
using Microsoft.Extensions.Logging;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;
using PhishingDetectionAPI.Infrastructure.MachineLearning;

/// <summary>
/// Serviço de gestão de usuários.
/// Implementa operações de CRUD e negócio específicas para usuários.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>Registra um novo usuário no sistema.</summary>
    public async Task<UserDto> RegisterUserAsync(string username, string email, string password, string fullName,
        string department = "", UserRole role = UserRole.User)
    {
        if (await _userRepository.UsernameExistsAsync(username))
            throw new InvalidOperationException("Nome de usuário já existe");

        if (await _userRepository.EmailExistsAsync(email))
            throw new InvalidOperationException("E-mail já está registrado");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            Department = department,
            Role = role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    /// <summary>Obtém um usuário pelo ID.</summary>
    public async Task<UserDto?> GetUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    /// <summary>Atualiza dados do usuário.</summary>
    public async Task<UserDto> UpdateUserAsync(int userId, string? fullName = null, string? department = null)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuário não encontrado");

        if (!string.IsNullOrWhiteSpace(fullName))
            user.FullName = fullName;

        if (!string.IsNullOrWhiteSpace(department))
            user.Department = department;

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    /// <summary>Lista todos os usuários (admin).</summary>
    public async Task<List<UserDto>> ListUsersAsync(int skip = 0, int take = 10)
    {
        var users = await _userRepository.GetAllAsync();
        return users
            .Skip(skip)
            .Take(take)
            .Select(u => _mapper.Map<UserDto>(u))
            .ToList();
    }

    /// <summary>Desativa um usuário.</summary>
    public async Task DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuário não encontrado");

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);
    }
}

/// <summary>
/// Serviço de auditoria.
/// Registra todas as ações do usuário para conformidade e rastreabilidade.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditService(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>Registra uma ação de auditoria.</summary>
    public async Task LogActionAsync(int? userId, string action, string resourceType, int? resourceId,
        string description, string ipAddress, string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    /// <summary>Obtém logs de auditoria de um usuário.</summary>
    public async Task<List<AuditLogDto>> GetUserAuditLogsAsync(int userId, int skip = 0, int take = 20)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(userId, skip, take);
        return logs
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Action = log.Action,
                ResourceType = log.ResourceType,
                ResourceId = log.ResourceId,
                Description = log.Description,
                IpAddress = log.IpAddress,
                CreatedAt = log.CreatedAt
            })
            .ToList();
    }
}

/// <summary>
/// Serviço de classificação de e-mail.
/// Orquestra a chamada ao microserviço Python de ML (Naive Bayes + Random Forest)
/// e converte a resposta para o contrato do backend.
/// </summary>
public class EmailClassificationService : IEmailClassificationService
{
    private readonly IMlServiceClient _mlServiceClient;
    private readonly ILogger<EmailClassificationService> _logger;

    public EmailClassificationService(IMlServiceClient mlServiceClient, ILogger<EmailClassificationService> logger)
    {
        _mlServiceClient = mlServiceClient ?? throw new ArgumentNullException(nameof(mlServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ClassificationResponse> ClassifyEmailAsync(ClassifyEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var mlRequest = new MlClassifyRequest
            {
                Subject = request.Subject,
                Body = request.Body,
                Sender = request.Sender,
                Recipients = request.Recipients
            };

            var result = await _mlServiceClient.ClassifyAsync(mlRequest);
            return MapToClassificationResponse(result);
        }
        catch (Exception ex)
        {
            // Falha no microserviço de ML não deve derrubar o fluxo de e-mail;
            // retorna classificação conservadora (suspicious) para revisão manual.
            _logger.LogError(ex, "Falha ao chamar o microserviço de ML para {Sender}", request.Sender);
            return new ClassificationResponse
            {
                Classification = "suspicious",
                Confidence = 0m,
                Indicators = new List<IndicatorDto>(),
                SuspiciousUrls = new List<string>(),
                Recommendation = "Não foi possível classificar automaticamente. Revise manualmente."
            };
        }
    }

    public async Task<List<ClassificationResponse>> ClassifyEmailsBatchAsync(List<ClassifyEmailRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        try
        {
            var mlRequests = requests.Select(r => new MlClassifyRequest
            {
                Subject = r.Subject,
                Body = r.Body,
                Sender = r.Sender,
                Recipients = r.Recipients
            }).ToList();

            var results = await _mlServiceClient.ClassifyBatchAsync(mlRequests);
            return results.Select(MapToClassificationResponse).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao chamar o microserviço de ML em lote");
            return requests.Select(_ => new ClassificationResponse
            {
                Classification = "suspicious",
                Confidence = 0m,
                Indicators = new List<IndicatorDto>(),
                SuspiciousUrls = new List<string>(),
                Recommendation = "Não foi possível classificar automaticamente. Revise manualmente."
            }).ToList();
        }
    }

    private static ClassificationResponse MapToClassificationResponse(MlClassificationResult result)
    {
        return new ClassificationResponse
        {
            Classification = result.Classification,
            Confidence = result.Confidence,
            Indicators = result.Indicators.Select(i => new IndicatorDto
            {
                Type = i.Type,
                Description = i.Description,
                RiskScore = i.RiskScore
            }).ToList(),
            SuspiciousUrls = result.SuspiciousUrls,
            Recommendation = result.Recommendation
        };
    }
}

/// <summary>
/// Serviço de consulta de e-mails já processados.
/// Usado pelo dashboard e relatórios para listar e-mails por classificação.
/// </summary>
public class EmailQueryService : IEmailQueryService
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public EmailQueryService(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>Obtém e-mails já processados por classificação, com paginação.</summary>
    public async Task<List<EmailDto>> GetByClassificationAsync(
        EmailClassification classification, int skip = 0, int take = 10)
    {
        var emails = await _emailRepository.GetByClassificationAsync(
            classification, Math.Max(0, skip), Math.Clamp(take, 1, 100));
        return _mapper.Map<List<EmailDto>>(emails);
    }
}

/// <summary>
/// Serviço de integração com e-mail.
/// Responsável por conexão IMAP/SMTP e recuperação de e-mails.
/// </summary>
public class EmailIntegrationService : IEmailIntegrationService
{
    public async Task<List<ClassifyEmailRequest>> FetchIncomingEmailsAsync(int limit = 100)
    {
        // TODO: Implementar conexão IMAP com MailKit
        await Task.Delay(500); // Simular latência

        return new List<ClassifyEmailRequest>();
    }

    public async Task<bool> TestConnectionAsync()
    {
        // TODO: Implementar teste de conexão IMAP
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> SendNotificationEmailAsync(string to, string subject, string body)
    {
        ArgumentNullException.ThrowIfNull(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(body);

        // TODO: Implementar envio de e-mail via SMTP
        await Task.Delay(100);
        return true;
    }
}
