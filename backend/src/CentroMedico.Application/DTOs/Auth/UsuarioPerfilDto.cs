namespace CentroMedico.Application.DTOs.Auth;

/// <summary>
/// DTO con datos básicos del perfil y rol del usuario conectado.
/// </summary>
public record UsuarioPerfilDto(
    int IdUsuario,
    string Email,
    string Nombre,
    string Apellido,
    string Rol
);
