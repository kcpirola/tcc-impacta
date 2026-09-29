namespace PhishingDetectionAPI.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>
/// Controlador de e-mails.
/// Fornece endpoints para classificação, listagem e feedback de e-mails.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EmailsController : ControllerBase
{
    private readonly IEmailClassificationService _classificationService;
    private readonly IEmailIntegrationService _emailIntegrationService;
    private readonly IEmailQueryService _emailQueryService;
    private readonly IAuditService _auditService;
    private readonly ILogger<EmailsController> _logger;

    public EmailsController(
        IEmailClassificationService classificationService,
        IEmailIntegrationService emailIntegrationService,
        IEmailQueryService emailQueryService,
        IAuditService auditService,
        ILogger<EmailsController> logger)
    {
        _classificationService = classificationService ??
            throw new ArgumentNullException(nameof(classificationService));
        _emailIntegrationService = emailIntegrationService ??
            throw new ArgumentNullException(nameof(emailIntegrationService));
        _emailQueryService = emailQueryService ??
            throw new ArgumentNullException(nameof(emailQueryService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Classifica um e-mail como safe, suspicious ou phishing.</summary>
    /// <param name="request">Dados do e-mail a classificar.</param>
    /// <returns>Resultado da classificação com confiança e indicadores.</returns>
    /// <response code="200">E-mail classificado com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="500">Erro ao processar classificação.</response>
    [HttpPost("classify")]
    [ProducesResponseType(typeof(ClassificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClassifyEmail([FromBody] ClassifyEmailRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Requisição inválida" });
            }

            if (string.IsNullOrWhiteSpace(request.Subject) || 
                string.IsNullOrWhiteSpace(request.Body) ||
                string.IsNullOrWhiteSpace(request.Sender))
            {
                return BadRequest(new 
                { 
                    message = "Subject, Body e Sender são obrigatórios" 
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var result = await _classificationService.ClassifyEmailAsync(request);

            // Registrar na auditoria
            if (int.TryParse(userId, out var userIdInt))
            {
                await _auditService.LogActionAsync(
                    userIdInt,
                    "classify_email",
                    "email",
                    null,
                    $"E-mail de {request.Sender} classificado como {result.Classification}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            }

            _logger.LogInformation($"E-mail classificado como {result.Classification}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao classificar e-mail");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao processar classificação" });
        }
    }

    /// <summary>Classifica múltiplos e-mails em lote.</summary>
    /// <param name="requests">Lista de e-mails a classificar.</param>
    /// <returns>Lista de resultados de classificação.</returns>
    /// <response code="200">E-mails classificados com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpPost("classify-batch")]
    [ProducesResponseType(typeof(List<ClassificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClassifyEmailsBatch([FromBody] List<ClassifyEmailRequest> requests)
    {
        try
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest(new { message = "Lista de e-mails não pode estar vazia" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var results = await _classificationService.ClassifyEmailsBatchAsync(requests);

            // Registrar na auditoria
            if (int.TryParse(userId, out var userIdInt))
            {
                await _auditService.LogActionAsync(
                    userIdInt,
                    "classify_emails_batch",
                    "email",
                    null,
                    $"Lote de {requests.Count} e-mails classificados",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            }

            _logger.LogInformation($"Lote de {requests.Count} e-mails classificados");
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao classificar lote de e-mails");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao processar lote de e-mails" });
        }
    }

    /// <summary>Envia feedback sobre uma classificação para melhorar o modelo.</summary>
    /// <param name="request">Feedback do usuário.</param>
    /// <returns>Confirmação do feedback recebido.</returns>
    /// <response code="200">Feedback recebido com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpPost("feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitFeedback([FromBody] EmailFeedbackRequest request)
    {
        try
        {
            if (request == null || request.EmailId <= 0)
            {
                return BadRequest(new { message = "EmailId é obrigatório e deve ser válido" });
            }

            if (string.IsNullOrWhiteSpace(request.Feedback) || 
                !new[] { "correct", "incorrect", "uncertain" }.Contains(request.Feedback.ToLower()))
            {
                return BadRequest(new 
                { 
                    message = "Feedback deve ser 'correct', 'incorrect' ou 'uncertain'" 
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Registrar na auditoria
            if (int.TryParse(userId, out var userIdInt))
            {
                await _auditService.LogActionAsync(
                    userIdInt,
                    "submit_feedback",
                    "email",
                    request.EmailId,
                    $"Feedback '{request.Feedback}' enviado para e-mail",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            }

            _logger.LogInformation($"Feedback recebido para e-mail {request.EmailId}: {request.Feedback}");
            
            return Ok(new { message = "Feedback recebido com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar feedback");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao processar feedback" });
        }
    }

    /// <summary>Conta os e-mails já analisados em cada classificação.</summary>
    /// <returns>Totais de seguros, suspeitos e phishing.</returns>
    /// <response code="200">Totais calculados.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(EmailStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            return Ok(await _emailQueryService.GetStatisticsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular as estatísticas de e-mails");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao calcular estatísticas" });
        }
    }

    /// <summary>Busca e-mails por classificação.</summary>
    /// <param name="classification">Classificação a buscar (safe, suspicious, phishing).</param>
    /// <param name="skip">Número de registros a pular (paginação).</param>
    /// <param name="take">Número de registros a retornar (paginação).</param>
    /// <returns>Lista de e-mails com a classificação especificada.</returns>
    /// <response code="200">E-mails encontrados.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpGet("by-classification/{classification}")]
    [ProducesResponseType(typeof(List<EmailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByClassification(
        string classification,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        try
        {
            if (!Enum.TryParse<EmailClassification>(classification, ignoreCase: true, out var parsedClassification))
            {
                return BadRequest(new { message = "Classificação inválida. Use safe, suspicious ou phishing." });
            }

            var emails = await _emailQueryService.GetByClassificationAsync(parsedClassification, skip, take);
            return Ok(emails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar e-mails por classificação");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao buscar e-mails" });
        }
    }
}
