using CentroMedico.Application.DTOs.Dashboard;
using CentroMedico.Application.Interfaces;
using CentroMedico.Application.Services;
using Xunit;

namespace CentroMedico.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly FakeDashboardRepository _fakeRepo;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _fakeRepo = new FakeDashboardRepository();
        _service = new DashboardService(_fakeRepo);
    }

    [Fact]
    public async Task CriterioAceptacion_ObtencionMetricasDashboardPrincipal_DebeRetornarKpisCalculados()
    {
        // Gherkin:
        // Escenario: Obtención de métricas del dashboard principal
        //   Dado que existen turnos registrados y cobros asentados en el mes
        //   Cuando el administrador consulta GET "/api/v1/dashboard/resumen"
        //   Entonces la API responde con código 200 OK
        //   Y el objeto contiene los contadores calculados y el monto de facturación mensual

        // Arrange
        _fakeRepo.ResumenKpis = new DashboardResumenDto(
            TurnosHoyTotal: 15,
            TurnosHoyPendientes: 10,
            TurnosHoyRealizados: 5,
            FacturacionMesActual: 125000.50m,
            PacientesActivosTotal: 48,
            TasaCancelacionMes: 6.25m
        );

        // Act
        var result = await _service.ObtenerResumenKpisAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.TurnosHoyTotal);
        Assert.Equal(10, result.TurnosHoyPendientes);
        Assert.Equal(5, result.TurnosHoyRealizados);
        Assert.Equal(125000.50m, result.FacturacionMesActual);
        Assert.Equal(48, result.PacientesActivosTotal);
        Assert.Equal(6.25m, result.TasaCancelacionMes);
    }

    [Fact]
    public async Task CriterioAceptacion_DistribucionTurnosPorEspecialidad_DebeDevolverListaConEspecialidadYTotal()
    {
        // Gherkin:
        // Escenario: Distribución de turnos por especialidad
        //   Dado que se registraron consultas en Traumatología y Clínica Médica
        //   Cuando se consulta GET "/api/v1/dashboard/turnos-por-especialidad"
        //   Entonces la API devuelve una lista con el nombre de la especialidad y el total de turnos

        // Arrange
        _fakeRepo.DistribucionEspecialidades = new List<TurnosPorEspecialidadDto>
        {
            new("Clínica Médica", 30, 60.00m),
            new("Traumatología", 20, 40.00m)
        };

        // Act
        var result = (await _service.ObtenerDistribucionEspecialidadesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Clínica Médica", result[0].Especialidad);
        Assert.Equal(30, result[0].TotalTurnos);
        Assert.Equal(60.00m, result[0].Porcentaje);

        Assert.Equal("Traumatología", result[1].Especialidad);
        Assert.Equal(20, result[1].TotalTurnos);
        Assert.Equal(40.00m, result[1].Porcentaje);
    }

    [Fact]
    public async Task ObtenerRecaudacionAnualAsync_CuandoNoSeEspecificaAnio_DebeUsarAnioActualYRetornar12Meses()
    {
        // Arrange
        var anioActual = DateTime.UtcNow.Year;
        _fakeRepo.RecaudacionAnual = Enumerable.Range(1, 12).Select(m => new RecaudacionMensualDto(
            Anio: anioActual,
            Mes: m,
            MesNombre: $"Mes {m}",
            TotalRecaudado: m * 1000m
        )).ToList();

        // Act
        var result = (await _service.ObtenerRecaudacionAnualAsync(null)).ToList();

        // Assert
        Assert.Equal(12, result.Count);
        Assert.All(result, r => Assert.Equal(anioActual, r.Anio));
        Assert.Equal(1000m, result[0].TotalRecaudado);
        Assert.Equal(12000m, result[11].TotalRecaudado);
    }

    [Fact]
    public async Task ObtenerProximosTurnosDelDiaAsync_DebeRetornarListadoLimitadoDeTurnos()
    {
        // Arrange
        _fakeRepo.ProximosTurnos = new List<TurnoResumenDto>
        {
            new(1, DateTime.UtcNow.AddHours(1), "Gómez, Lucía", "López, Juan", "Pendiente"),
            new(2, DateTime.UtcNow.AddHours(2), "Pérez, Carlos", "Rodríguez, María", "Pendiente")
        };

        // Act
        var result = (await _service.ObtenerProximosTurnosDelDiaAsync(10)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Gómez, Lucía", result[0].PacienteNombre);
        Assert.Equal("López, Juan", result[0].MedicoNombre);
    }
}

public class FakeDashboardRepository : IDashboardRepository
{
    public DashboardResumenDto ResumenKpis { get; set; } = new(0, 0, 0, 0m, 0, 0m);
    public List<TurnosPorEspecialidadDto> DistribucionEspecialidades { get; set; } = new();
    public List<RecaudacionMensualDto> RecaudacionAnual { get; set; } = new();
    public List<TurnoResumenDto> ProximosTurnos { get; set; } = new();

    public Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ResumenKpis);
    }

    public Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(
        int? mes = null,
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TurnosPorEspecialidadDto> list = DistribucionEspecialidades;
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionPorMesesAsync(
        int anio,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RecaudacionMensualDto> list = RecaudacionAnual;
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosAsync(
        int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TurnoResumenDto> list = ProximosTurnos.Take(cantidad).ToList();
        return Task.FromResult(list);
    }
}
