namespace CentroMedico.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un cobro o transacción de pago efectuada por un paciente.
/// </summary>
public class Pago
{
    public int IdPago { get; set; }
    public int IdPaciente { get; set; }
    public int? IdTurno { get; set; }
    public byte IdConcepto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Monto { get; set; }
    public byte IdEstado { get; set; }
    public string? MetodoPago { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Paciente titular al que corresponde el cobro.
    /// </summary>
    public Paciente Paciente { get; set; } = null!;

    /// <summary>
    /// Concepto o práctica médica facturada.
    /// </summary>
    public Concepto Concepto { get; set; } = null!;

    /// <summary>
    /// Estado actual de la transacción del cobro.
    /// </summary>
    public PagoEstado PagoEstado { get; set; } = null!;

    /// <summary>
    /// Turno médico asociado si la transacción salda una consulta específica (opcional).
    /// </summary>
    public Turno? Turno { get; set; }
}
