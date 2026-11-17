using CentroMedico.Application.DTOs.Dashboard;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Enums;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Repositorio de analítica de alto rendimiento para el Dashboard administrativo mediante EF Core.
/// Todas las consultas se ejecutan con AsNoTracking() y proyecciones directas.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private static readonly string[] MesesNombres =
    {
        "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
        "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
    };

    private readonly CentroMedicoDbContext _context;

    public DashboardRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;
        var hoyInicio = new DateTime(ahora.Year, ahora.Month, ahora.Day, 0, 0, 0, DateTimeKind.Utc);
        var hoyFin = hoyInicio.AddDays(1);

        var mesInicio = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var mesFin = mesInicio.AddMonths(1);

        // Turnos del día de hoy
        var turnosHoy = await _context.Turnos
            .AsNoTracking()
            .Where(t => t.FechaTurno >= hoyInicio && t.FechaTurno < hoyFin)
            .Select(t => t.IdEstado)
            .ToListAsync(cancellationToken);

        var turnosHoyTotal = turnosHoy.Count;
        var turnosHoyPendientes = turnosHoy.Count(e => e == (short)EstadoTurnoEnum.Pendiente);
        var turnosHoyRealizados = turnosHoy.Count(e => e == (short)EstadoTurnoEnum.Realizado);

        // Facturación del mes actual (pagos en estado 1 = Aprobado)
        var facturacionMes = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.IdEstado == (byte)EstadoPagoEnum.Aprobado && p.Fecha >= mesInicio && p.Fecha < mesFin)
            .SumAsync(p => (decimal?)p.Monto, cancellationToken) ?? 0m;

        // Pacientes activos totales
        var pacientesActivos = await _context.Pacientes
            .AsNoTracking()
            .CountAsync(p => p.Activo, cancellationToken);

        // Tasa de cancelación del mes actual
        var turnosMes = await _context.Turnos
            .AsNoTracking()
            .Where(t => t.FechaTurno >= mesInicio && t.FechaTurno < mesFin)
            .Select(t => t.IdEstado)
            .ToListAsync(cancellationToken);

        var totalTurnosMes = turnosMes.Count;
        var turnosCanceladosMes = turnosMes.Count(e => e == (short)EstadoTurnoEnum.Cancelado);
        var tasaCancelacion = totalTurnosMes > 0
            ? Math.Round((decimal)turnosCanceladosMes / totalTurnosMes * 100m, 2)
            : 0m;

        return new DashboardResumenDto(
            TurnosHoyTotal: turnosHoyTotal,
            TurnosHoyPendientes: turnosHoyPendientes,
            TurnosHoyRealizados: turnosHoyRealizados,
            FacturacionMesActual: facturacionMes,
            PacientesActivosTotal: pacientesActivos,
            TasaCancelacionMes: tasaCancelacion
        );
    }

    public async Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(
        int? mes = null,
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos
            .AsNoTracking()
            .AsQueryable();

        if (mes.HasValue)
        {
            query = query.Where(t => t.FechaTurno.Month == mes.Value);
        }

        if (anio.HasValue)
        {
            query = query.Where(t => t.FechaTurno.Year == anio.Value);
        }

        var turnosPorMedico = await query
            .GroupBy(t => t.IdMedico)
            .Select(g => new { IdMedico = g.Key, Total = g.Count() })
            .ToListAsync(cancellationToken);

        if (turnosPorMedico.Count == 0)
        {
            return Array.Empty<TurnosPorEspecialidadDto>();
        }

        var medicosIds = turnosPorMedico.Select(x => x.IdMedico).ToList();
        var medicosEspecialidades = await _context.MedicosEspecialidades
            .Include(me => me.Especialidad)
            .AsNoTracking()
            .Where(me => medicosIds.Contains(me.IdMedico))
            .ToListAsync(cancellationToken);

        var acumulador = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var tpm in turnosPorMedico)
        {
            var especialidadesDelMedico = medicosEspecialidades
                .Where(me => me.IdMedico == tpm.IdMedico)
                .Select(me => me.Especialidad.Nombre)
                .Distinct()
                .ToList();

            if (especialidadesDelMedico.Count > 0)
            {
                foreach (var esp in especialidadesDelMedico)
                {
                    acumulador[esp] = acumulador.GetValueOrDefault(esp, 0) + tpm.Total;
                }
            }
            else
            {
                acumulador["Sin Especialidad"] = acumulador.GetValueOrDefault("Sin Especialidad", 0) + tpm.Total;
            }
        }

        var granTotal = acumulador.Values.Sum();

        return acumulador
            .Select(kvp => new TurnosPorEspecialidadDto(
                Especialidad: kvp.Key,
                TotalTurnos: kvp.Value,
                Porcentaje: granTotal > 0 ? Math.Round((decimal)kvp.Value / granTotal * 100m, 2) : 0m
            ))
            .OrderByDescending(x => x.TotalTurnos)
            .ThenBy(x => x.Especialidad)
            .ToList();
    }

    public async Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionPorMesesAsync(
        int anio,
        CancellationToken cancellationToken = default)
    {
        var pagosDelAnio = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.IdEstado == (byte)EstadoPagoEnum.Aprobado && p.Fecha.Year == anio)
            .GroupBy(p => p.Fecha.Month)
            .Select(g => new { Mes = g.Key, Total = g.Sum(p => p.Monto) })
            .ToListAsync(cancellationToken);

        var pagosDict = pagosDelAnio.ToDictionary(x => x.Mes, x => x.Total);
        var resultado = new List<RecaudacionMensualDto>(12);

        for (int mes = 1; mes <= 12; mes++)
        {
            var total = pagosDict.GetValueOrDefault(mes, 0m);
            resultado.Add(new RecaudacionMensualDto(anio, mes, MesesNombres[mes], total));
        }

        return resultado;
    }

    public async Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosAsync(
        int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        var limite = cantidad > 0 ? cantidad : 10;
        var ahora = DateTime.UtcNow;
        var hoyInicio = new DateTime(ahora.Year, ahora.Month, ahora.Day, 0, 0, 0, DateTimeKind.Utc);

        return await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.TurnoEstado)
            .AsNoTracking()
            .Where(t => t.FechaTurno >= hoyInicio)
            .OrderBy(t => t.FechaTurno)
            .Take(limite)
            .Select(t => new TurnoResumenDto(
                t.IdTurno,
                t.FechaTurno,
                t.Paciente != null ? $"{t.Paciente.Apellido}, {t.Paciente.Nombre}" : string.Empty,
                t.Medico != null ? $"{t.Medico.Apellido}, {t.Medico.Nombre}" : string.Empty,
                t.TurnoEstado != null ? t.TurnoEstado.Descripcion : string.Empty
            ))
            .ToListAsync(cancellationToken);
    }
}
