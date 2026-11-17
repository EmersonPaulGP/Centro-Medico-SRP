using CentroMedico.Application.DTOs.Dashboard;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de persistencia analítica y agregaciones de alto rendimiento para el Dashboard.
/// </summary>
public interface IDashboardRepository
{
    Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(int? mes = null, int? anio = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionPorMesesAsync(int anio, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosAsync(int cantidad = 10, CancellationToken cancellationToken = default);
}
