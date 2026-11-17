namespace CentroMedico.Application.DTOs.Pagos;

/// <summary>
/// DTO con el detalle completo de un pago y sus entidades relacionadas.
/// </summary>
public record PagoDetalleDto(
    int IdPago,
    DateTime Fecha,
    int IdPaciente,
    string PacienteNombreCompleto,
    string PacienteDni,
    int? IdTurno,
    byte IdConcepto,
    string ConceptoDescripcion,
    decimal Monto,
    byte IdEstado,
    string EstadoDescripcion,
    string? MetodoPago,
    string? Observacion
);
