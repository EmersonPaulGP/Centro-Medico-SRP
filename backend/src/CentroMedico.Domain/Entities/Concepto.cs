namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de catálogo para conceptos de facturación / cobro.
/// </summary>
public class Concepto
{
    public byte IdConcepto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
