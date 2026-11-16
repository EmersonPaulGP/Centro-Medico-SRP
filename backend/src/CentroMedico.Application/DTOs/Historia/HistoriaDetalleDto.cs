namespace CentroMedico.Application.DTOs.Historia;

/// <summary>
/// DTO de respuesta detallada para una evolución en la historia clínica.
/// </summary>
public record HistoriaDetalleDto(
    int IdHistoria,
    DateTime FechaHistoria,
    int IdPaciente,
    string PacienteNombreCompleto,
    string PacienteDni,
    int IdMedico,
    string MedicoNombreCompleto,
    string MedicoMatricula,
    int? IdTurno,
    string? Diagnostico,
    string Observacion,
    DateTime FechaAlta
);
