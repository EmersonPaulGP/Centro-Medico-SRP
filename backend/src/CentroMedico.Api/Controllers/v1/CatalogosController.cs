using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Catalogos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints para la consulta de catálogos y tablas maestras del sistema médico.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    /// <summary>
    /// Obtiene el catálogo de países ordenados alfabéticamente.
    /// </summary>
    [HttpGet("paises")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaisDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPaises(CancellationToken cancellationToken)
    {
        var paises = await _catalogoService.ObtenerPaisesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<PaisDto>>.Ok(paises, "Listado de países obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el catálogo de especialidades médicas (por defecto solo especialidades activas).
    /// </summary>
    [HttpGet("especialidades")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EspecialidadDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEspecialidades([FromQuery] bool soloActivas = true, CancellationToken cancellationToken = default)
    {
        var especialidades = await _catalogoService.ObtenerEspecialidadesAsync(soloActivas, cancellationToken);
        return Ok(ApiResponse<IEnumerable<EspecialidadDto>>.Ok(especialidades, "Listado de especialidades médicas obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el catálogo de estados de turno médico.
    /// </summary>
    [HttpGet("estados-turno")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CatalogoGenericoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEstadosTurno(CancellationToken cancellationToken)
    {
        var estados = await _catalogoService.ObtenerEstadosTurnoAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<CatalogoGenericoDto>>.Ok(estados, "Listado de estados de turnos obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el catálogo de conceptos de facturación / pago.
    /// </summary>
    [HttpGet("conceptos-pago")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CatalogoGenericoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerConceptosPago(CancellationToken cancellationToken)
    {
        var conceptos = await _catalogoService.ObtenerConceptosPagoAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<CatalogoGenericoDto>>.Ok(conceptos, "Listado de conceptos de pago obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el catálogo de estados de cobro / pago.
    /// </summary>
    [HttpGet("estados-pago")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CatalogoGenericoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerEstadosPago(CancellationToken cancellationToken)
    {
        var estados = await _catalogoService.ObtenerEstadosPagoAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<CatalogoGenericoDto>>.Ok(estados, "Listado de estados de pago obtenido exitosamente."));
    }

    /// <summary>
    /// Registra una nueva especialidad médica en el sistema.
    /// </summary>
    [HttpPost("especialidades")]
    [ProducesResponseType(typeof(ApiResponse<EspecialidadDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CrearEspecialidad([FromBody] CrearEspecialidadDto dto, CancellationToken cancellationToken)
    {
        var nuevaEspecialidad = await _catalogoService.CrearEspecialidadAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EspecialidadDto>.Ok(nuevaEspecialidad, "Especialidad médica registrada exitosamente."));
    }
}
