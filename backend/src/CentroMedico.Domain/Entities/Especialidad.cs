namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de catálogo para especialidades médicas del policlínico.
/// </summary>
public class Especialidad
{
    public int IdEspecialidad { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<MedicoEspecialidad> MedicosEspecialidades { get; set; } = new List<MedicoEspecialidad>();
}
