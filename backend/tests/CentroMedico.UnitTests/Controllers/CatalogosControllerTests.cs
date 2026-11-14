using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Catalogos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class CatalogosControllerTests
{
    private readonly FakeCatalogoService _fakeService;
    private readonly CatalogosController _controller;

    public CatalogosControllerTests()
    {
        _fakeService = new FakeCatalogoService();
        _controller = new CatalogosController(_fakeService);
    }

    [Fact]
    public async Task ObtenerPaises_DebeRetornar200Ok_ConApiResponse()
    {
        // Act
        var actionResult = await _controller.ObtenerPaises(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<PaisDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task ObtenerEspecialidades_DebeRetornar200Ok_ConApiResponse()
    {
        // Act
        var actionResult = await _controller.ObtenerEspecialidades(soloActivas: true, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<EspecialidadDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task CrearEspecialidad_DebeRetornar201Created_ConApiResponse()
    {
        // Arrange
        var dto = new CrearEspecialidadDto("Oftalmología", "Salud visual");

        // Act
        var actionResult = await _controller.CrearEspecialidad(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<EspecialidadDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("Oftalmología", apiResponse.Data!.Nombre);
    }
}

public class FakeCatalogoService : ICatalogoService
{
    public Task<IEnumerable<PaisDto>> ObtenerPaisesAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<PaisDto> list = new List<PaisDto> { new("ARG", "Argentina"), new("PER", "Perú") };
        return Task.FromResult(list);
    }

    public Task<IEnumerable<EspecialidadDto>> ObtenerEspecialidadesAsync(bool soloActivas = true, CancellationToken cancellationToken = default)
    {
        IEnumerable<EspecialidadDto> list = new List<EspecialidadDto> { new(1, "Cardiología", "Corazón", true) };
        return Task.FromResult(list);
    }

    public Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<CatalogoGenericoDto> list = new List<CatalogoGenericoDto> { new(0, "Pendiente") };
        return Task.FromResult(list);
    }

    public Task<IEnumerable<CatalogoGenericoDto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<CatalogoGenericoDto> list = new List<CatalogoGenericoDto> { new(1, "Consulta") };
        return Task.FromResult(list);
    }

    public Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<CatalogoGenericoDto> list = new List<CatalogoGenericoDto> { new(0, "Pendiente") };
        return Task.FromResult(list);
    }

    public Task<EspecialidadDto> CrearEspecialidadAsync(CrearEspecialidadDto dto, CancellationToken cancellationToken = default)
    {
        var result = new EspecialidadDto(1, dto.Nombre, dto.Descripcion, true);
        return Task.FromResult(result);
    }
}
