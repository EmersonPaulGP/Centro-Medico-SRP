namespace CentroMedico.Application.DTOs.Auth;

/// <summary>
/// DTO de respuesta con el token JWT firmado y datos del usuario autenticado.
/// </summary>
public record LoginResponseDto(
    string Token,
    string TokenType,
    int ExpiraEnSegundos,
    UsuarioPerfilDto Usuario
);
