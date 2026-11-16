using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Historia;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class HistoriasClinicasControllerTests
{
    private readonly FakeHistoriaService _fakeService;
    private readonly HistoriasClinicasController _controller;

    public HistoriasClinicasControllerTests()
    {
        _fakeService = new FakeHistoriaService();
        _controller = new HistoriasClinicasController(_fakeService);
    }

    [Fact]
    public async Task RegistrarEvolucion_DebeRetornar201Created_ConLocationHeader()
    {
        var dto = new CrearHistoriaDto(5, 1, 20, "Faringitis", "Reposo 48hs y amoxicilina");
        var actionResult = await _controller.RegistrarEvolucion(dto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(HistoriasClinicasController.ObtenerPorId), createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<HistoriaDetalleDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdHistoria);
    }

    [Fact]
    public async Task ObtenerPorId_DebeRetornar200Ok_ConHistoriaDetalleDto()
    {
        var actionResult = await _controller.ObtenerPorId(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<HistoriaDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdHistoria);
    }

    [Fact]
    public async Task ObtenerHistoriaPaciente_DebeRetornar200Ok_ConPagedResult()
    {
        var filtro = new HistoriaFiltroDto();
        var actionResult = await _controller.ObtenerHistoriaPaciente(5, filtro, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<HistoriaDetalleDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Single(apiResponse.Data!.Items);
    }
}

public class FakeHistoriaService : IHistoriaService
{
    public Task<HistoriaDetalleDto> RegistrarEvolucionAsync(CrearHistoriaDto dto, CancellationToken cancellationToken = default)
    {
        var detalle = new HistoriaDetalleDto(
            1,
            DateTime.UtcNow,
            dto.IdPaciente,
            "Gómez, Lucía",
            "30111222",
            dto.IdMedico,
            "Quito, Esteban",
            "MN-12345",
            dto.IdTurno,
            dto.Diagnostico,
            dto.Observacion,
            DateTime.UtcNow
        );
        return Task.FromResult(detalle);
    }

    public Task<HistoriaDetalleDto> ObtenerPorIdAsync(int idHistoria, CancellationToken cancellationToken = default)
    {
        var detalle = new HistoriaDetalleDto(
            idHistoria,
            DateTime.UtcNow,
            5,
            "Gómez, Lucía",
            "30111222",
            1,
            "Quito, Esteban",
            "MN-12345",
            null,
            "Diagnóstico de prueba",
            "Observación de prueba para consulta",
            DateTime.UtcNow
        );
        return Task.FromResult(detalle);
    }

    public Task<PagedResult<HistoriaDetalleDto>> ObtenerHistoriaPacienteAsync(int idPaciente, HistoriaFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var items = new List<HistoriaDetalleDto>
        {
            new(1, DateTime.UtcNow, idPaciente, "Gómez, Lucía", "30111222", 1, "Quito, Esteban", "MN-12345", null, "Control", "Evolución favorable", DateTime.UtcNow)
        };
        var paged = new PagedResult<HistoriaDetalleDto>(items, 1, 1, 15);
        return Task.FromResult(paged);
    }
}
