namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa una cita médica agendada entre un paciente y un profesional médico.
/// </summary>
public class Turno
{
    public int IdTurno { get; set; }
    public int IdPaciente { get; set; }
    public int IdMedico { get; set; }
    public DateTime FechaTurno { get; set; }
    public short IdEstado { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Paciente asignado al turno.
    /// </summary>
    public Paciente Paciente { get; set; } = null!;

    /// <summary>
    /// Médico profesional que atenderá la consulta.
    /// </summary>
    public Medico Medico { get; set; } = null!;

    /// <summary>
    /// Estado actual de la cita médica.
    /// </summary>
    public TurnoEstado TurnoEstado { get; set; } = null!;
}
