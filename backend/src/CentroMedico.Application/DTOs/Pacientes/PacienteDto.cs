namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO de respuesta con la información completa del paciente.
/// </summary>
public record PacienteDto(
    int IdPaciente,
    string Dni,
    string Nombre,
    string Apellido,
    DateOnly FNacimiento,
    int Edad,
    string Domicilio,
    string IdPais,
    string PaisNombre,
    string? Telefono,
    string Email,
    string? Observacion,
    bool Activo,
    DateTime FechaAlta,
    DateTime? FechaModificacion
);
