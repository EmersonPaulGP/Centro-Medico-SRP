namespace CentroMedico.Application.DTOs.Pagos;

/// <summary>
/// DTO para el registro de un nuevo cobro o transacción de pago.
/// </summary>
public record RegistrarPagoDto(
    int IdPaciente,
    int? IdTurno,
    byte IdConcepto,
    decimal Monto,
    string? MetodoPago,
    string? Observacion
);
