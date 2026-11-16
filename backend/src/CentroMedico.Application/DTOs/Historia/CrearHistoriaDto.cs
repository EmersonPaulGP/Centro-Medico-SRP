namespace CentroMedico.Application.DTOs.Historia;

/// <summary>
/// DTO de entrada para asentar una nueva evolución médica en la historia clínica.
/// </summary>
public record CrearHistoriaDto(
    int IdPaciente,
    int IdMedico,
    int? IdTurno,
    string? Diagnostico,
    string Observacion
);
