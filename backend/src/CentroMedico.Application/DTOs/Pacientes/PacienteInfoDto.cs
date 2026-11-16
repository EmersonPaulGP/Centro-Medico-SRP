namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO de respuesta que representa los antecedentes clínicos de un paciente.
/// </summary>
public record PacienteInfoDto(
    int IdPaciente,
    bool Diabetico,
    bool Implantes,
    string? Alergias,
    string? GrupoSanguineo,
    DateTime? FechaModificacion
);
