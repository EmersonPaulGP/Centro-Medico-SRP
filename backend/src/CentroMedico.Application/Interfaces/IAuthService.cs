using CentroMedico.Application.DTOs.Auth;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para autenticación, autorización y gestión de sesiones de usuario.
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto> RegistrarUsuarioAsync(RegistroUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioPerfilDto> ObtenerPerfilUsuarioAsync(int idUsuario, CancellationToken cancellationToken = default);
}
