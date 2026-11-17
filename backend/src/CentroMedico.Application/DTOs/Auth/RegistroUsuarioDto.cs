namespace CentroMedico.Application.DTOs.Auth;

/// <summary>
/// DTO para la creación y alta de un nuevo usuario en el sistema.
/// </summary>
public record RegistroUsuarioDto(
    string Email,
    string Password,
    string Nombre,
    string Apellido,
    string Rol
);
