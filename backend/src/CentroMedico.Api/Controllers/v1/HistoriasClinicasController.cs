using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Historia;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la gestión y consulta de la historia clínica y notas de evolución médica.
/// </summary>
[ApiController]
[Route("api/v1/historias")]
[Produces("application/json")]
public class HistoriasClinicasController : ControllerBase
{
    private readonly IHistoriaService _historiaService;

    public HistoriasClinicasController(IHistoriaService historiaService)
    {
        _historiaService = historiaService;
    }

    /// <summary>
    /// Registra una nueva evolución médica en la historia clínica.
    /// Si se proporciona un idTurno, el turno asociado cambiará automáticamente a estado Realizado (1).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HistoriaDetalleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarEvolucion([FromBody] CrearHistoriaDto dto, CancellationToken cancellationToken)
    {
        var creada = await _historiaService.RegistrarEvolucionAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = creada.IdHistoria },
            ApiResponse<HistoriaDetalleDto>.Ok(creada, "Evolución clínica registrada exitosamente.")
        );
    }

    /// <summary>
    /// Obtiene el detalle de una evolución clínica por su identificador único.
    /// </summary>
    [HttpGet("{id:int}", Name = nameof(ObtenerPorId))]
    [ProducesResponseType(typeof(ApiResponse<HistoriaDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var historia = await _historiaService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<HistoriaDetalleDto>.Ok(historia, "Evolución clínica obtenida exitosamente."));
    }

    /// <summary>
    /// Consulta la línea de tiempo de evoluciones de la historia clínica de un paciente, ordenadas cronológicamente de forma descendente.
    /// </summary>
    [HttpGet("/api/v1/pacientes/{idPaciente:int}/historia")]
    [HttpGet("paciente/{idPaciente:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<HistoriaDetalleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistoriaPaciente(
        [FromRoute] int idPaciente,
        [FromQuery] HistoriaFiltroDto filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await _historiaService.ObtenerHistoriaPacienteAsync(idPaciente, filtro, cancellationToken);
        return Ok(ApiResponse<PagedResult<HistoriaDetalleDto>>.Ok(resultado, "Historia clínica del paciente obtenida exitosamente."));
    }
}
