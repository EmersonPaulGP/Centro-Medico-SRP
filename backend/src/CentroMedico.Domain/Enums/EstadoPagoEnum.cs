namespace CentroMedico.Domain.Enums;

/// <summary>
/// Catálogo de estados posibles para una transacción de pago o cobro.
/// </summary>
public enum EstadoPagoEnum : byte
{
    Pendiente = 0,
    Aprobado = 1,
    Rechazado = 2,
    Reembolsado = 3
}
