namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO para la actualización de datos de contacto y domicilio de un paciente.
/// </summary>
public record ActualizarPacienteDto(
    string Nombre,
    string Apellido,
    DateOnly FNacimiento,
    string Domicilio,
    string IdPais,
    string? Telefono,
    string Email,
    string? Observacion = null
);
