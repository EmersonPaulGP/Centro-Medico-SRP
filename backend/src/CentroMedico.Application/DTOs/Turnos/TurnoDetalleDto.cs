namespace CentroMedico.Application.DTOs.Turnos;

/// <summary>
/// DTO detallado de respuesta para citas y turnos médicos.
/// </summary>
public record TurnoDetalleDto(
    int IdTurno,
    DateTime FechaTurno,
    short IdEstado,
    string EstadoDescripcion,
    int IdPaciente,
    string PacienteNombreCompleto,
    string PacienteDni,
    string? PacienteTelefono,
    int IdMedico,
    string MedicoNombreCompleto,
    string MedicoMatricula,
    string? Observacion,
    DateTime FechaAlta,
    DateTime? FechaModificacion
);
