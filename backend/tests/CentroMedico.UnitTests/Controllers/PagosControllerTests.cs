using CentroMedico.Api.Controllers.v1;
using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pagos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CentroMedico.UnitTests.Controllers;

public class PagosControllerTests
{
    private readonly FakePagoService _fakeService;
    private readonly PagosController _controller;

    public PagosControllerTests()
    {
        _fakeService = new FakePagoService();
        _controller = new PagosController(_fakeService);
    }

    [Fact]
    public async Task RegistrarPago_DebeRetornar201Created_ConLocationHeader()
    {
        // Arrange
        var dto = new RegistrarPagoDto(5, null, 1, 5000.00m, "Efectivo", "Cobro consulta general");

        // Act
        var actionResult = await _controller.RegistrarPago(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(nameof(PagosController.ObtenerPorId), createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<PagoDetalleDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdPago);
        Assert.Equal(5000.00m, apiResponse.Data.Monto);
    }

    [Fact]
    public async Task ObtenerPorId_DebeRetornar200Ok_ConPagoDetalleDto()
    {
        // Act
        var actionResult = await _controller.ObtenerPorId(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagoDetalleDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.IdPago);
    }

    [Fact]
    public async Task ObtenerComprobante_DebeRetornar200Ok_ConComprobantePagoDto()
    {
        // Act
        var actionResult = await _controller.ObtenerComprobante(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<ComprobantePagoDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(1, apiResponse.Data!.NumeroComprobante);
        Assert.Equal("Centro Médico SRP", apiResponse.Data.CentroMedicoNombre);
    }

    [Fact]
    public async Task ListarPagos_DebeRetornar200Ok_ConPagedResult()
    {
        // Arrange
        var filtro = new PagoFiltroDto();

        // Act
        var actionResult = await _controller.ListarPagos(filtro, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<PagoDetalleDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Single(apiResponse.Data!.Items);
    }

    [Fact]
    public async Task ListarPagosPorPaciente_DebeRetornar200Ok_ConPagedResult()
    {
        // Arrange
        var filtro = new PagoFiltroDto();

        // Act
        var actionResult = await _controller.ListarPagosPorPaciente(5, filtro, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var apiResponse = Assert.IsType<ApiResponse<PagedResult<PagoDetalleDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }
}

public class FakePagoService : IPagoService
{
    public Task<PagoDetalleDto> RegistrarPagoAsync(RegistrarPagoDto dto, CancellationToken cancellationToken = default)
    {
        var detalle = new PagoDetalleDto(
            1,
            DateTime.UtcNow,
            dto.IdPaciente,
            "Gómez, Lucía",
            "30111222",
            dto.IdTurno,
            dto.IdConcepto,
            "Consulta Médica General",
            dto.Monto,
            1,
            "Aprobado",
            dto.MetodoPago,
            dto.Observacion
        );
        return Task.FromResult(detalle);
    }

    public Task<PagoDetalleDto> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var detalle = new PagoDetalleDto(
            idPago,
            DateTime.UtcNow,
            5,
            "Gómez, Lucía",
            "30111222",
            null,
            1,
            "Consulta Médica General",
            5000.00m,
            1,
            "Aprobado",
            "Efectivo",
            null
        );
        return Task.FromResult(detalle);
    }

    public Task<ComprobantePagoDto> ObtenerComprobanteAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var comprobante = new ComprobantePagoDto(
            NumeroComprobante: idPago,
            FechaEmision: DateTime.UtcNow,
            CentroMedicoNombre: "Centro Médico SRP",
            CentroMedicoCuit: "30-71234567-9",
            CentroMedicoDireccion: "Av. Principal 1234, CABA",
            IdPaciente: 5,
            PacienteNombre: "Gómez, Lucía",
            PacienteDni: "30111222",
            PacienteDomicilio: "Av. Santa Fe 1234",
            IdConcepto: 1,
            Concepto: "Consulta Médica General",
            IdTurno: null,
            Subtotal: 5000.00m,
            Total: 5000.00m,
            MetodoPago: "Efectivo",
            Observacion: null
        );
        return Task.FromResult(comprobante);
    }

    public Task<PagedResult<PagoDetalleDto>> ListarPagosAsync(PagoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var items = new List<PagoDetalleDto>
        {
            new(
                1,
                DateTime.UtcNow,
                filtro.IdPaciente ?? 5,
                "Gómez, Lucía",
                "30111222",
                null,
                1,
                "Consulta Médica General",
                5000.00m,
                1,
                "Aprobado",
                "Efectivo",
                null
            )
        };
        var paged = new PagedResult<PagoDetalleDto>(items, 1, 1, 20);
        return Task.FromResult(paged);
    }
}
