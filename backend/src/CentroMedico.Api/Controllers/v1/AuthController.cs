using System.Security.Claims;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Auth;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la autenticación, inicio de sesión y gestión de sesiones de usuario.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión con correo electrónico y contraseña, devolviendo un token JWT firmado.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var respuesta = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(respuesta, "Sesión iniciada exitosamente."));
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema con su correspondiente rol RBAC.
    /// Restringido a Administrador.
    /// </summary>
    [HttpPost("registro")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(ApiResponse<UsuarioPerfilDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDto dto, CancellationToken cancellationToken)
    {
        var creado = await _authService.RegistrarUsuarioAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioPerfilDto>.Ok(creado, "Usuario registrado exitosamente."));
    }

    /// <summary>
    /// Obtiene los datos del perfil y rol del usuario actualmente autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioPerfilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPerfilActual(CancellationToken cancellationToken)
    {
        var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(idUsuarioStr) || !int.TryParse(idUsuarioStr, out var idUsuario))
        {
            return Unauthorized(ApiResponse<object>.Fail("Token no contiene una identidad de usuario válida."));
        }

        var perfil = await _authService.ObtenerPerfilUsuarioAsync(idUsuario, cancellationToken);
        return Ok(ApiResponse<UsuarioPerfilDto>.Ok(perfil, "Perfil de usuario obtenido exitosamente."));
    }
}
