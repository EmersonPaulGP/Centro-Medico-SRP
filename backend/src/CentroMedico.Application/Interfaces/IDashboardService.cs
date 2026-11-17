using CentroMedico.Application.DTOs.Dashboard;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso y lógica de negocio para la consulta analítica del Dashboard.
/// </summary>
public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(int? mes = null, int? anio = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionAnualAsync(int? anio = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosDelDiaAsync(int cantidad = 10, CancellationToken cancellationToken = default);
}
