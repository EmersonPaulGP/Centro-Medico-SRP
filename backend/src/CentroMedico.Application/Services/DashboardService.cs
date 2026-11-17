using CentroMedico.Application.DTOs.Dashboard;
using CentroMedico.Application.Interfaces;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso analíticos y KPIs del Dashboard administrativo.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default)
    {
        return await _dashboardRepository.ObtenerResumenKpisAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(
        int? mes = null,
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        return await _dashboardRepository.ObtenerDistribucionEspecialidadesAsync(mes, anio, cancellationToken);
    }

    public async Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionAnualAsync(
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        var anioEfectivo = anio.HasValue && anio.Value > 1900
            ? anio.Value
            : DateTime.UtcNow.Year;

        return await _dashboardRepository.ObtenerRecaudacionPorMesesAsync(anioEfectivo, cancellationToken);
    }

    public async Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosDelDiaAsync(
        int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var limite = cantidad > 0 ? Math.Min(cantidad, 50) : 10;
        return await _dashboardRepository.ObtenerProximosTurnosAsync(limite, cancellationToken);
    }
}
