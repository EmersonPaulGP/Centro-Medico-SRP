using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Dashboard;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class DashboardControllerTests
{
    private readonly FakeDashboardService _fakeService;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _fakeService = new FakeDashboardService();
        _controller = new DashboardController(_fakeService);
    }

    [Fact]
    public async Task ObtenerResumen_DebeRetornar200Ok_ConDashboardResumenDto()
    {
        // Act
        var actionResult = await _controller.ObtenerResumen(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<DashboardResumenDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(10, apiResponse.Data!.TurnosHoyTotal);
        Assert.Equal(50000m, apiResponse.Data.FacturacionMesActual);
    }

    [Fact]
    public async Task ObtenerTurnosPorEspecialidad_DebeRetornar200Ok_ConListaDistribucion()
    {
        // Act
        var actionResult = await _controller.ObtenerTurnosPorEspecialidad(null, null, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<TurnosPorEspecialidadDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.NotEmpty(apiResponse.Data!);
    }

    [Fact]
    public async Task ObtenerRecaudacionMensual_DebeRetornar200Ok_ConListaMeses()
    {
        // Act
        var actionResult = await _controller.ObtenerRecaudacionMensual(2026, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<RecaudacionMensualDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(12, apiResponse.Data!.Count);
    }

    [Fact]
    public async Task ObtenerProximosTurnos_DebeRetornar200Ok_ConListaTurnosResumen()
    {
        // Act
        var actionResult = await _controller.ObtenerProximosTurnos(5, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<TurnoResumenDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }
}

public class FakeDashboardService : IDashboardService
{
    public Task<DashboardResumenDto> ObtenerResumenKpisAsync(CancellationToken cancellationToken = default)
    {
        var resumen = new DashboardResumenDto(10, 6, 4, 50000m, 120, 5.0m);
        return Task.FromResult(resumen);
    }

    public Task<IReadOnlyList<TurnosPorEspecialidadDto>> ObtenerDistribucionEspecialidadesAsync(
        int? mes = null,
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TurnosPorEspecialidadDto> lista = new List<TurnosPorEspecialidadDto>
        {
            new("Clínica Médica", 20, 66.67m),
            new("Traumatología", 10, 33.33m)
        };
        return Task.FromResult(lista);
    }

    public Task<IReadOnlyList<RecaudacionMensualDto>> ObtenerRecaudacionAnualAsync(
        int? anio = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RecaudacionMensualDto> lista = Enumerable.Range(1, 12).Select(m => new RecaudacionMensualDto(
            Anio: anio ?? 2026,
            Mes: m,
            MesNombre: $"Mes {m}",
            TotalRecaudado: 1000m * m
        )).ToList();
        return Task.FromResult(lista);
    }

    public Task<IReadOnlyList<TurnoResumenDto>> ObtenerProximosTurnosDelDiaAsync(
        int cantidad = 10,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TurnoResumenDto> lista = new List<TurnoResumenDto>
        {
            new(1, DateTime.UtcNow.AddMinutes(30), "Gómez, Lucía", "López, Juan", "Pendiente")
        };
        return Task.FromResult(lista);
    }
}
