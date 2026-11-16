namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa una entrada o nota de evolución en la historia clínica del paciente.
/// </summary>
public class Historia
{
    public int IdHistoria { get; set; }
    public int IdPaciente { get; set; }
    public int IdMedico { get; set; }
    public int? IdTurno { get; set; }
    public DateTime FechaHistoria { get; set; } = DateTime.UtcNow;
    public string? Diagnostico { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Paciente titular de la historia clínica.
    /// </summary>
    public Paciente Paciente { get; set; } = null!;

    /// <summary>
    /// Médico profesional que asentó la evolución clínica.
    /// </summary>
    public Medico Medico { get; set; } = null!;

    /// <summary>
    /// Turno de atención médica que originó la consulta (opcional).
    /// </summary>
    public Turno? Turno { get; set; }
}
