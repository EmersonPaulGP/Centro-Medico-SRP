using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class PacientesControllerTests
{
    private readonly FakePacienteService _fakeService;
    private readonly PacientesController _controller;

    public PacientesControllerTests()
    {
        _fakeService = new FakePacienteService();
        _controller = new PacientesController(_fakeService);
    }

    [Fact]
    public async Task ListarPacientes_DebeRetornar200Ok_ConPagedResult()
    {
        var filtro = new PacienteFiltroDto();
        var actionResult = await _controller.ListarPacientes(filtro, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<PacienteDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task ObtenerPacientePorId_DebeRetornar200Ok_ConPacienteDto()
    {
        var actionResult = await _controller.ObtenerPacientePorId(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PacienteDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdPaciente);
    }

    [Fact]
    public async Task ObtenerPacientePorDni_DebeRetornar200Ok_ConPacienteDto()
    {
        var actionResult = await _controller.ObtenerPacientePorDni("12345678", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PacienteDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("12345678", apiResponse.Data!.Dni);
    }

    [Fact]
    public async Task CrearPaciente_DebeRetornar201Created_ConCreatedAtAction()
    {
        var dto = new CrearPacienteDto(
            "45678912",
            "Carlos",
            "Tevez",
            new DateOnly(1984, 2, 5),
            "Fuerte Apache",
            "ARG",
            null,
            "carlos@boca.com"
        );

        var actionResult = await _controller.CrearPaciente(dto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PacienteDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("45678912", apiResponse.Data!.Dni);
    }

    [Fact]
    public async Task ActualizarPaciente_DebeRetornar200Ok()
    {
        var dto = new ActualizarPacienteDto(
            "Carlos Alberto",
            "Tevez",
            new DateOnly(1984, 2, 5),
            "Buenos Aires",
            "ARG",
            "11223344",
            "carlos.alberto@boca.com"
        );

        var actionResult = await _controller.ActualizarPaciente(1, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PacienteDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Carlos Alberto", apiResponse.Data!.Nombre);
    }

    [Fact]
    public async Task CambiarEstado_DebeRetornar200Ok()
    {
        var dto = new CambiarEstadoPacienteDto(false);
        var actionResult = await _controller.CambiarEstado(1, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task DarDeBaja_DebeRetornar204NoContent()
    {
        var actionResult = await _controller.DarDeBaja(1, CancellationToken.None);

        var noContentResult = Assert.IsType<NoContentResult>(actionResult);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }
}

public class FakePacienteService : IPacienteService
{
    public Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var paciente = new PacienteDto(
            1,
            dto.Dni,
            dto.Nombre,
            dto.Apellido,
            dto.FNacimiento,
            40,
            dto.Domicilio,
            dto.IdPais,
            "Argentina",
            dto.Telefono,
            dto.Email,
            dto.Observacion,
            true,
            DateTime.UtcNow,
            null
        );
        return Task.FromResult(paciente);
    }

    public Task<PacienteDto> ActualizarAsync(int idPaciente, ActualizarPacienteDto dto, CancellationToken cancellationToken = default)
    {
        var paciente = new PacienteDto(
            idPaciente,
            "45678912",
            dto.Nombre,
            dto.Apellido,
            dto.FNacimiento,
            40,
            dto.Domicilio,
            dto.IdPais,
            "Argentina",
            dto.Telefono,
            dto.Email,
            dto.Observacion,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
        return Task.FromResult(paciente);
    }

    public Task<PacienteDto> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var paciente = new PacienteDto(
            idPaciente,
            "45678912",
            "Juan",
            "Pérez",
            new DateOnly(1990, 1, 1),
            34,
            "Calle 1",
            "ARG",
            "Argentina",
            null,
            "juan@test.com",
            null,
            true,
            DateTime.UtcNow,
            null
        );
        return Task.FromResult(paciente);
    }

    public Task<PacienteDto> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var paciente = new PacienteDto(
            1,
            dni,
            "Juan",
            "Pérez",
            new DateOnly(1990, 1, 1),
            34,
            "Calle 1",
            "ARG",
            "Argentina",
            null,
            "juan@test.com",
            null,
            true,
            DateTime.UtcNow,
            null
        );
        return Task.FromResult(paciente);
    }

    public Task<PagedResult<PacienteDto>> ListarAsync(PacienteFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var items = new List<PacienteDto>
        {
            new(1, "12345678", "Juan", "Pérez", new DateOnly(1990, 1, 1), 34, "Calle 1", "ARG", "Argentina", null, "juan@test.com", null, true, DateTime.UtcNow, null)
        };
        var paged = new PagedResult<PacienteDto>(items, 1, 1, 10);
        return Task.FromResult(paged);
    }

    public Task BajaLogicaAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CambiarEstadoActivoAsync(int idPaciente, bool activo, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
