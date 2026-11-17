using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la gestión y agendamiento de citas y turnos médicos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = $"{Roles.Administrador},{Roles.Recepcionista},{Roles.Medico}")]
[Produces("application/json")]
public class TurnosController : ControllerBase
{
    private readonly ITurnoService _turnoService;

    public TurnosController(ITurnoService turnoService)
    {
        _turnoService = turnoService;
    }

    /// <summary>
    /// Consulta la agenda de turnos con filtros por médico, paciente, rango de fechas y estado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TurnoDetalleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAgenda([FromQuery] TurnoFiltroAgendaDto filtro, CancellationToken cancellationToken)
    {
        var resultado = await _turnoService.ObtenerAgendaAsync(filtro, cancellationToken);
        return Ok(ApiResponse<PagedResult<TurnoDetalleDto>>.Ok(resultado, "Agenda de turnos obtenida exitosamente."));
    }

    /// <summary>
    /// Obtiene el detalle completo de un turno médico por su identificador único.
    /// </summary>
    [HttpGet("{id:int}", Name = nameof(ObtenerTurnoPorId))]
    [ProducesResponseType(typeof(ApiResponse<TurnoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerTurnoPorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var turno = await _turnoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TurnoDetalleDto>.Ok(turno, "Turno obtenido exitosamente."));
    }

    /// <summary>
    /// Agenda una nueva cita médica validando que médico y paciente estén activos y sin conflicto de horario.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TurnoDetalleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AgendarTurno([FromBody] AgendarTurnoDto dto, CancellationToken cancellationToken)
    {
        var creado = await _turnoService.AgendarAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerTurnoPorId),
            new { id = creado.IdTurno },
            ApiResponse<TurnoDetalleDto>.Ok(creado, "Turno agendado exitosamente.")
        );
    }

    /// <summary>
    /// Actualiza el estado de una cita médica (ej. Realizado, Postergado, etc.).
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<TurnoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(
        [FromRoute] int id,
        [FromBody] ActualizarEstadoTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _turnoService.CambiarEstadoAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TurnoDetalleDto>.Ok(actualizado, "Estado del turno actualizado exitosamente."));
    }

    /// <summary>
    /// Cancela un turno registrando obligatoriamente el motivo de cancelación y liberando el horario del médico.
    /// </summary>
    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<TurnoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarTurno(
        [FromRoute] int id,
        [FromBody] CancelarTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var cancelado = await _turnoService.CancelarTurnoAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<TurnoDetalleDto>.Ok(cancelado, "Turno cancelado exitosamente. El horario ha sido liberado."));
    }
}
