namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa a un paciente del centro médico.
/// </summary>
public class Paciente
{
    public int IdPaciente { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public DateOnly FNacimiento { get; set; }
    public string Domicilio { get; set; } = string.Empty;
    public string IdPais { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Navegación al país de procedencia (catálogo ISO-3).
    /// </summary>
    public Pais Pais { get; set; } = null!;

    /// <summary>
    /// Edad calculada a partir de la fecha de nacimiento.
    /// </summary>
    public int Edad
    {
        get
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var edad = hoy.Year - FNacimiento.Year;
            if (FNacimiento > hoy.AddYears(-edad))
            {
                edad--;
            }
            return edad >= 0 ? edad : 0;
        }
    }
}
