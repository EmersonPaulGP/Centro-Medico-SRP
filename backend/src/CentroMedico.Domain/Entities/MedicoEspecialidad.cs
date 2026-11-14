namespace CentroMedico.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación N a N entre Médicos y Especialidades.
/// </summary>
public class MedicoEspecialidad
{
    public int IdMedico { get; set; }
    public int IdEspecialidad { get; set; }
    public string? Descripcion { get; set; }

    public Medico Medico { get; set; } = null!;
    public Especialidad Especialidad { get; set; } = null!;
}
