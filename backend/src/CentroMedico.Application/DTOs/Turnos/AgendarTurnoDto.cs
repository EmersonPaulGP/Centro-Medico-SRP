namespace CentroMedico.Application.DTOs.Turnos;

/// <summary>
/// DTO de entrada para agendar una nueva cita o turno médico.
/// </summary>
public record AgendarTurnoDto(
    int IdPaciente,
    int IdMedico,
    DateTime FechaTurno,
    string? Observacion = null
);
