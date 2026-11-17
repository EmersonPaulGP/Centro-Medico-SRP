namespace CentroMedico.Application.DTOs.Pagos;

/// <summary>
/// DTO que representa el comprobante digital de cobro o recibo emitido al paciente.
/// </summary>
public record ComprobantePagoDto(
    int NumeroComprobante,
    DateTime FechaEmision,
    string CentroMedicoNombre,
    string CentroMedicoCuit,
    string CentroMedicoDireccion,
    int IdPaciente,
    string PacienteNombre,
    string PacienteDni,
    string PacienteDomicilio,
    byte IdConcepto,
    string Concepto,
    int? IdTurno,
    decimal Subtotal,
    decimal Total,
    string MetodoPago,
    string? Observacion
);
