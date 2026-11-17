namespace CentroMedico.Application.DTOs.Dashboard;

/// <summary>
/// KPIs y métricas consolidadas del dashboard administrativo en tiempo real.
/// </summary>
public record DashboardResumenDto(
    int TurnosHoyTotal,
    int TurnosHoyPendientes,
    int TurnosHoyRealizados,
    decimal FacturacionMesActual,
    int PacientesActivosTotal,
    decimal TasaCancelacionMes
);
