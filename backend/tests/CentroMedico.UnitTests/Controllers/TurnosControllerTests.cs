using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class TurnosControllerTests
{
    private readonly FakeTurnoService _fakeService;
    private readonly TurnosController _controller;

    public TurnosControllerTests()
    {
        _fakeService = new FakeTurnoService();
        _controller = new TurnosController(_fakeService);
    }

    [Fact]
    public async Task ListarAgenda_DebeRetornar200Ok_ConPagedResult()
    {
        var filtro = new TurnoFiltroAgendaDto();
        var actionResult = await _controller.ListarAgenda(filtro, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<TurnoDetalleDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task ObtenerTurnoPorId_DebeRetornar200Ok_ConTurnoDetalleDto()
    {
        var actionResult = await _controller.ObtenerTurnoPorId(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<TurnoDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdTurno);
    }

    [Fact]
    public async Task AgendarTurno_DebeRetornar201Created_ConLocationHeader()
    {
        var dto = new AgendarTurnoDto(5, 1, DateTime.UtcNow.AddDays(1), "Cita");
        var actionResult = await _controller.AgendarTurno(dto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(TurnosController.ObtenerTurnoPorId), createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<TurnoDetalleDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task CambiarEstado_DebeRetornar200Ok()
    {
        var dto = new ActualizarEstadoTurnoDto(1, "Atendido");
        var actionResult = await _controller.CambiarEstado(1, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<TurnoDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(1, apiResponse.Data!.IdEstado);
    }

    [Fact]
    public async Task CancelarTurno_DebeRetornar200Ok()
    {
        var dto = new CancelarTurnoDto("Motivo de fuerza mayor");
        var actionResult = await _controller.CancelarTurno(1, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<TurnoDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(2, apiResponse.Data!.IdEstado);
    }
}

public class FakeTurnoService : ITurnoService
{
    public Task<TurnoDetalleDto> AgendarAsync(AgendarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        var turno = new TurnoDetalleDto(
            1,
            dto.FechaTurno,
            0,
            "Pendiente",
            dto.IdPaciente,
            "Pérez, Juan",
            "12345678",
            null,
            dto.IdMedico,
            "Quito, Esteban",
            "MN-100",
            dto.Observacion,
            DateTime.UtcNow,
            null
        );
        return Task.FromResult(turno);
    }

    public Task<TurnoDetalleDto> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        var turno = new TurnoDetalleDto(
            idTurno,
            DateTime.UtcNow.AddDays(1),
            0,
            "Pendiente",
            5,
            "Pérez, Juan",
            "12345678",
            null,
            1,
            "Quito, Esteban",
            "MN-100",
            null,
            DateTime.UtcNow,
            null
        );
        return Task.FromResult(turno);
    }

    public Task<PagedResult<TurnoDetalleDto>> ObtenerAgendaAsync(TurnoFiltroAgendaDto filtro, CancellationToken cancellationToken = default)
    {
        var items = new List<TurnoDetalleDto>
        {
            new(1, DateTime.UtcNow.AddDays(1), 0, "Pendiente", 5, "Pérez, Juan", "12345678", null, 1, "Quito, Esteban", "MN-100", null, DateTime.UtcNow, null)
        };
        var paged = new PagedResult<TurnoDetalleDto>(items, 1, 1, 20);
        return Task.FromResult(paged);
    }

    public Task<TurnoDetalleDto> CambiarEstadoAsync(int idTurno, ActualizarEstadoTurnoDto dto, CancellationToken cancellationToken = default)
    {
        var turno = new TurnoDetalleDto(
            idTurno,
            DateTime.UtcNow.AddDays(1),
            dto.NuevoEstado,
            "Actualizado",
            5,
            "Pérez, Juan",
            "12345678",
            null,
            1,
            "Quito, Esteban",
            "MN-100",
            dto.Observacion,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        return Task.FromResult(turno);
    }

    public Task<TurnoDetalleDto> CancelarTurnoAsync(int idTurno, CancelarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        var turno = new TurnoDetalleDto(
            idTurno,
            DateTime.UtcNow.AddDays(1),
            2,
            "Cancelado",
            5,
            "Pérez, Juan",
            "12345678",
            null,
            1,
            "Quito, Esteban",
            "MN-100",
            $"Cancelado: {dto.MotivoCancelacion}",
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        return Task.FromResult(turno);
    }
}
