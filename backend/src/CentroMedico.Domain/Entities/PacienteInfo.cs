namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa la ficha clínica base y antecedentes médicos de un paciente.
/// </summary>
public class PacienteInfo
{
    public int IdPaciente { get; set; }
    public bool Diabetico { get; set; }
    public bool Implantes { get; set; }
    public string? Alergias { get; set; }
    public string? GrupoSanguineo { get; set; }
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Navegación al paciente asociado (relación 1 a 1).
    /// </summary>
    public Paciente Paciente { get; set; } = null!;
}
