using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class MedicosControllerTests
{
    private readonly FakeMedicoService _fakeService;
    private readonly MedicosController _controller;

    public MedicosControllerTests()
    {
        _fakeService = new FakeMedicoService();
        _controller = new MedicosController(_fakeService);
    }

    [Fact]
    public async Task ListarMedicos_DebeRetornar200Ok_ConPagedResult()
    {
        var filtro = new MedicoFiltroDto();
        var actionResult = await _controller.ListarMedicos(filtro, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<MedicoDetalleDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task ObtenerMedicoPorId_DebeRetornar200Ok_ConMedicoDetalle()
    {
        var actionResult = await _controller.ObtenerMedicoPorId(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<MedicoDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdMedico);
    }

    [Fact]
    public async Task CrearMedico_DebeRetornar201Created_ConCreatedAtAction()
    {
        var dto = new CrearMedicoDto(
            "MN-55555",
            "55555555",
            "Mariana",
            "Herrera",
            null,
            null,
            new List<AsignarEspecialidadDto> { new(1) }
        );

        var actionResult = await _controller.CrearMedico(dto, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<MedicoDetalleDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("MN-55555", apiResponse.Data!.Matricula);
    }

    [Fact]
    public async Task CambiarEstado_DebeRetornar200Ok()
    {
        var dto = new CambiarEstadoMedicoDto(false);
        var actionResult = await _controller.CambiarEstado(1, dto, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }
}

public class FakeMedicoService : IMedicoService
{
    public Task<MedicoDetalleDto> CrearAsync(CrearMedicoDto dto, CancellationToken cancellationToken = default)
    {
        var detalle = new MedicoDetalleDto(
            1,
            dto.Matricula,
            dto.Dni,
            dto.Nombre,
            dto.Apellido,
            dto.Telefono,
            dto.Email,
            true,
            DateTime.UtcNow,
            null,
            new List<EspecialidadAsignadaDto> { new(1, "Clínica Médica") }
        );
        return Task.FromResult(detalle);
    }

    public Task<MedicoDetalleDto> ActualizarAsync(int idMedico, ActualizarMedicoDto dto, CancellationToken cancellationToken = default)
    {
        var detalle = new MedicoDetalleDto(
            idMedico,
            "MN-9999",
            "99999999",
            dto.Nombre,
            dto.Apellido,
            dto.Telefono,
            dto.Email,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            new List<EspecialidadAsignadaDto>()
        );
        return Task.FromResult(detalle);
    }

    public Task<PagedResult<MedicoDetalleDto>> ListarAsync(MedicoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var items = new List<MedicoDetalleDto>
        {
            new(1, "MN-1", "D1", "A", "B", null, null, true, DateTime.UtcNow, null, new List<EspecialidadAsignadaDto>())
        };
        var paged = new PagedResult<MedicoDetalleDto>(items, 1, 1, 10);
        return Task.FromResult(paged);
    }

    public Task<MedicoDetalleDto> ObtenerPorIdAsync(int idMedico, CancellationToken cancellationToken = default)
    {
        var detalle = new MedicoDetalleDto(
            idMedico,
            "MN-1",
            "D1",
            "A",
            "B",
            null,
            null,
            true,
            DateTime.UtcNow,
            null,
            new List<EspecialidadAsignadaDto>()
        );
        return Task.FromResult(detalle);
    }

    public Task CambiarEstadoActivoAsync(int idMedico, bool activo, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<MedicoDetalleDto> ActualizarEspecialidadesAsync(int idMedico, List<AsignarEspecialidadDto> especialidades, CancellationToken cancellationToken = default)
    {
        var detalle = new MedicoDetalleDto(
            idMedico,
            "MN-1",
            "D1",
            "A",
            "B",
            null,
            null,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow,
            new List<EspecialidadAsignadaDto>()
        );
        return Task.FromResult(detalle);
    }
}
