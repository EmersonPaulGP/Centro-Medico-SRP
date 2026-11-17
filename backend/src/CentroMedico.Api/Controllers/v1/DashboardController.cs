using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Dashboard;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la consulta analítica, métricas en tiempo real y KPIs del Dashboard administrativo.
/// Requiere rol de Administrador.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = Roles.Administrador)]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Consulta el resumen consolidado de KPIs y métricas del centro médico (turnos hoy, facturación del mes, pacientes activos y tasa de cancelación).
    /// </summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumen(CancellationToken cancellationToken)
    {
        var resumen = await _dashboardService.ObtenerResumenKpisAsync(cancellationToken);
        return Ok(ApiResponse<DashboardResumenDto>.Ok(resumen, "Resumen de métricas del dashboard obtenido exitosamente."));
    }

    /// <summary>
    /// Consulta la distribución de turnos agrupados por especialidad médica para alimentar gráficos estadísticos.
    /// </summary>
    [HttpGet("turnos-por-especialidad")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TurnosPorEspecialidadDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTurnosPorEspecialidad(
        [FromQuery] int? mes,
        [FromQuery] int? anio,
        CancellationToken cancellationToken)
    {
        var distribucion = await _dashboardService.ObtenerDistribucionEspecialidadesAsync(mes, anio, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TurnosPorEspecialidadDto>>.Ok(distribucion, "Distribución de turnos por especialidad obtenida exitosamente."));
    }

    /// <summary>
    /// Consulta el histórico de recaudación mensual agrupado por mes para un año determinado (por defecto, año corriente).
    /// </summary>
    [HttpGet("recaudacion-mensual")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RecaudacionMensualDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRecaudacionMensual(
        [FromQuery] int? anio,
        CancellationToken cancellationToken)
    {
        var recaudacion = await _dashboardService.ObtenerRecaudacionAnualAsync(anio, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RecaudacionMensualDto>>.Ok(recaudacion, "Histórico de recaudación mensual obtenido exitosamente."));
    }

    /// <summary>
    /// Consulta el listado rápido de las próximas citas del día para la recepción del centro médico.
    /// </summary>
    [HttpGet("proximos-turnos")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TurnoResumenDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerProximosTurnos(
        [FromQuery] int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var turnos = await _dashboardService.ObtenerProximosTurnosDelDiaAsync(cantidad, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TurnoResumenDto>>.Ok(turnos, "Próximos turnos obtenidos exitosamente."));
    }
}
