using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pagos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la administración de cobros, facturación y comprobantes digitales.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    /// <summary>
    /// Registra un nuevo cobro o transacción de pago para un paciente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PagoDetalleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoDto dto, CancellationToken cancellationToken)
    {
        var creado = await _pagoService.RegistrarPagoAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = creado.IdPago },
            ApiResponse<PagoDetalleDto>.Ok(creado, "Cobro registrado exitosamente.")
        );
    }

    /// <summary>
    /// Obtiene el detalle de un cobro por su identificador único.
    /// </summary>
    [HttpGet("{id:int}", Name = nameof(ObtenerPorId))]
    [ProducesResponseType(typeof(ApiResponse<PagoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var pago = await _pagoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PagoDetalleDto>.Ok(pago, "Pago obtenido exitosamente."));
    }

    /// <summary>
    /// Emite los datos fiscales y detalle del comprobante de pago digital para el paciente.
    /// </summary>
    [HttpGet("{id:int}/comprobante")]
    [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerComprobante([FromRoute] int id, CancellationToken cancellationToken)
    {
        var comprobante = await _pagoService.ObtenerComprobanteAsync(id, cancellationToken);
        return Ok(ApiResponse<ComprobantePagoDto>.Ok(comprobante, "Comprobante emitido exitosamente."));
    }

    /// <summary>
    /// Consulta el listado de cobros con filtros avanzados por paciente, concepto, estado y rango de fechas (arqueo de caja).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PagoDetalleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPagos([FromQuery] PagoFiltroDto filtro, CancellationToken cancellationToken)
    {
        var resultado = await _pagoService.ListarPagosAsync(filtro, cancellationToken);
        return Ok(ApiResponse<PagedResult<PagoDetalleDto>>.Ok(resultado, "Listado de pagos obtenido exitosamente."));
    }

    /// <summary>
    /// Consulta el historial de pagos asociados directamente a un paciente.
    /// </summary>
    [HttpGet("/api/v1/pacientes/{idPaciente:int}/pagos")]
    [HttpGet("paciente/{idPaciente:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PagoDetalleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPagosPorPaciente(
        [FromRoute] int idPaciente,
        [FromQuery] PagoFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var filtroConPaciente = filtro with { IdPaciente = idPaciente };
        var resultado = await _pagoService.ListarPagosAsync(filtroConPaciente, cancellationToken);
        return Ok(ApiResponse<PagedResult<PagoDetalleDto>>.Ok(resultado, "Historial de pagos del paciente obtenido exitosamente."));
    }
}
