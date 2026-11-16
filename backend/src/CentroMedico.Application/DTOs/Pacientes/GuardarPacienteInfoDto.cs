namespace CentroMedico.Application.DTOs.Pacientes;

/// <summary>
/// DTO para la creación o actualización de antecedentes clínicos de un paciente.
/// </summary>
public record GuardarPacienteInfoDto(
    bool Diabetico,
    bool Implantes,
    string? Alergias,
    string? GrupoSanguineo
);
