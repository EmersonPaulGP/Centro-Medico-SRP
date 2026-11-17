using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para el registro y consulta de pagos y transacciones de cobro.
/// </summary>
public interface IPagoRepository
{
    Task<Pago?> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default);
    Task<Pago?> ObtenerPorIdConDetalleAsync(int idPago, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Pago> Items, int Total)> ObtenerFiltradosAsync(
        int? idPaciente,
        byte? idConcepto,
        byte? idEstado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default);
    Task<Pago> AgregarAsync(Pago pago, CancellationToken cancellationToken = default);
}
