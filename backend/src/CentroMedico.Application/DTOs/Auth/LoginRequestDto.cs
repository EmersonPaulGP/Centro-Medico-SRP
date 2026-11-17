namespace CentroMedico.Application.DTOs.Auth;

/// <summary>
/// DTO con credenciales para inicio de sesión.
/// </summary>
public record LoginRequestDto(
    string Email,
    string Password
);
