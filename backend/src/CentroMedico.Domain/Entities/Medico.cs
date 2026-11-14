namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa a un profesional médico del centro de salud.
/// </summary>
public class Medico
{
    public int IdMedico { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Especialidades asignadas al médico (relación N:N).
    /// </summary>
    public ICollection<MedicoEspecialidad> Especialidades { get; set; } = new List<MedicoEspecialidad>();
}
