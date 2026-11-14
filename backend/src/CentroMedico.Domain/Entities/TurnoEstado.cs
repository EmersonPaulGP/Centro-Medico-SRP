namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de catálogo para los estados posibles de un turno médico.
/// </summary>
public class TurnoEstado
{
    public short IdEstado { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
