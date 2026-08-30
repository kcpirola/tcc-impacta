namespace PhishingDetectionAPI.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>
/// Controlador de autenticação.
/// Fornece endpoints para login e geração de tokens JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService, 
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Autentica um usuário e retorna um token JWT.</summary>
    /// <param name="request">Credenciais do usuário (username e password).</param>
    /// <returns>Token JWT se bem-sucedido.</returns>
    /// <response code="200">Autenticação bem-sucedida, token retornado.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Credenciais incorretas.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { message = "Requisição inválida" });
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username e password são obrigatórios" });
            }

            var response = await _authService.AuthenticateAsync(request);

            // Registrar login na auditoria
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            
            if (response.User != null)
            {
                await _auditService.LogActionAsync(
                    response.User.Id,
                    "login",
                    "user",
                    response.User.Id,
                    $"Usuário {response.User.Username} realizou login",
                    ipAddress,
                    userAgent);
            }

            _logger.LogInformation($"Login bem-sucedido para usuário: {request.Username}");
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Tentativa de login falhada: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>Valida um token JWT.</summary>
    /// <param name="token">Token JWT a validar.</param>
    /// <returns>Status de validação do token.</returns>
    /// <response code="200">Token é válido.</response>
    /// <response code="401">Token é inválido ou expirou.</response>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public IActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Token é obrigatório" });
            }

            var isValid = _authService.ValidateToken(token);
            
            if (isValid)
            {
                return Ok(new { message = "Token é válido" });
            }

            return Unauthorized(new { message = "Token é inválido ou expirou" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>Retorna informações do usuário autenticado (teste de token).</summary>
    /// <returns>Informações do usuário autenticado.</returns>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                userId = userId,
                username = username,
                email = email,
                role = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário atual");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro interno do servidor" });
        }
    }
}
