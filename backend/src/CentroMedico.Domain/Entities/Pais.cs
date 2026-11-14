namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de catálogo para países (norma ISO-3).
/// </summary>
public class Pais
{
    public string IdPais { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
