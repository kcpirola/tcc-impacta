namespace PhishingDetectionAPI.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhishingDetectionAPI.Application.Dtos;
using PhishingDetectionAPI.Domain.Entities;
using PhishingDetectionAPI.Domain.Interfaces;

/// <summary>Autocadastro de usuários.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public UsersController(IUserService userService, IAuditService auditService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>Cria a conta de quem está chamando, com a senha escolhida por ele.</summary>
    /// <remarks>Aberto, sem login. O papel é sempre "user"; promover a analyst/admin é feito no banco.</remarks>
    /// <response code="201">Conta criada.</response>
    /// <response code="400">Dados inválidos (ex.: senha com menos de 6 caracteres).</response>
    /// <response code="409">Usuário ou e-mail já cadastrado.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
    {
        UserDto criado;
        try
        {
            criado = await _userService.RegisterUserAsync(
                request.Username.Trim(),
                request.Email.Trim(),
                request.Password,
                request.FullName.Trim(),
                request.Department.Trim(),
                UserRole.User);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        await _auditService.LogActionAsync(
            criado.Id,
            "register",
            "user",
            criado.Id,
            $"Usuário {criado.Username} se cadastrou",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        return StatusCode(StatusCodes.Status201Created, criado);
    }
}
