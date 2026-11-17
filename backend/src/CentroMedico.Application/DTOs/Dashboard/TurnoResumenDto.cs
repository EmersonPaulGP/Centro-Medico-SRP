namespace CentroMedico.Application.DTOs.Dashboard;

/// <summary>
/// Resumen ligero de una cita médica para listados rápidos en recepción.
/// </summary>
public record TurnoResumenDto(
    int IdTurno,
    DateTime FechaTurno,
    string PacienteNombre,
    string MedicoNombre,
    string EstadoDescripcion
);
