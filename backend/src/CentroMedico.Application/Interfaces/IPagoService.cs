using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pagos;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para la gestión y registro de cobros, pagos y comprobantes.
/// </summary>
public interface IPagoService
{
    Task<PagoDetalleDto> RegistrarPagoAsync(RegistrarPagoDto dto, CancellationToken cancellationToken = default);
    Task<PagoDetalleDto> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default);
    Task<ComprobantePagoDto> ObtenerComprobanteAsync(int idPago, CancellationToken cancellationToken = default);
    Task<PagedResult<PagoDetalleDto>> ListarPagosAsync(PagoFiltroDto filtro, CancellationToken cancellationToken = default);
}
