namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de catálogo para los estados posibles de un pago / cobro.
/// </summary>
public class PagoEstado
{
    public byte IdEstado { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
