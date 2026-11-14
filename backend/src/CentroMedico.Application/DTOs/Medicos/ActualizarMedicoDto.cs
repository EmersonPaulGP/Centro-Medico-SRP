namespace CentroMedico.Application.DTOs.Medicos;

public record ActualizarMedicoDto(
    string Nombre,
    string Apellido,
    string? Telefono = null,
    string? Email = null
);
