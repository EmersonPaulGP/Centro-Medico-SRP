namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO para el registro inicial de un paciente en el centro médico.
/// </summary>
public record CrearPacienteDto(
    string Dni,
    string Nombre,
    string Apellido,
    DateOnly FNacimiento,
    string Domicilio,
    string IdPais,
    string? Telefono,
    string Email,
    string? Observacion = null
);
