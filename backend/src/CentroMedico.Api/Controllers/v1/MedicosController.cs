using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la administración de médicos y especialidades profesionales.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class MedicosController : ControllerBase
{
    private readonly IMedicoService _medicoService;

    public MedicosController(IMedicoService medicoService)
    {
        _medicoService = medicoService;
    }

    /// <summary>
    /// Consulta el listado paginado de médicos con filtros opcionales de búsqueda, especialidad y estado activo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicoDetalleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMedicos([FromQuery] MedicoFiltroDto filtro, CancellationToken cancellationToken)
    {
        var resultado = await _medicoService.ListarAsync(filtro, cancellationToken);
        return Ok(ApiResponse<PagedResult<MedicoDetalleDto>>.Ok(resultado, "Listado de médicos obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene el perfil detallado de un médico por su identificador único, incluyendo sus especialidades.
    /// </summary>
    [HttpGet("{id:int}", Name = nameof(ObtenerMedicoPorId))]
    [ProducesResponseType(typeof(ApiResponse<MedicoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerMedicoPorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var medico = await _medicoService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<MedicoDetalleDto>.Ok(medico, "Médico encontrado exitosamente."));
    }

    /// <summary>
    /// Registra un nuevo profesional médico con al menos una especialidad médica inicial.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MedicoDetalleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CrearMedico([FromBody] CrearMedicoDto dto, CancellationToken cancellationToken)
    {
        var creado = await _medicoService.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerMedicoPorId),
            new { id = creado.IdMedico },
            ApiResponse<MedicoDetalleDto>.Ok(creado, "Médico registrado exitosamente.")
        );
    }

    /// <summary>
    /// Actualiza la información personal y de contacto de un médico.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MedicoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ActualizarMedico(
        [FromRoute] int id,
        [FromBody] ActualizarMedicoDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _medicoService.ActualizarAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<MedicoDetalleDto>.Ok(actualizado, "Datos del médico actualizados exitosamente."));
    }

    /// <summary>
    /// Realiza la baja lógica o reactivación del profesional médico.
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        [FromRoute] int id,
        [FromBody] CambiarEstadoMedicoDto dto,
        CancellationToken cancellationToken)
    {
        await _medicoService.CambiarEstadoActivoAsync(id, dto.Activo, cancellationToken);
        var mensaje = dto.Activo
            ? "El médico ha sido reactivado exitosamente."
            : "Baja lógica realizada exitosamente. El médico ya no estará disponible para nuevas citas.";

        return Ok(ApiResponse<object>.Ok(new { idMedico = id, activo = dto.Activo }, mensaje));
    }

    /// <summary>
    /// Asigna o reemplaza las especialidades médicas vinculadas a un médico.
    /// </summary>
    [HttpPut("{id:int}/especialidades")]
    [ProducesResponseType(typeof(ApiResponse<MedicoDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarEspecialidades(
        [FromRoute] int id,
        [FromBody] List<AsignarEspecialidadDto> especialidades,
        CancellationToken cancellationToken)
    {
        var actualizado = await _medicoService.ActualizarEspecialidadesAsync(id, especialidades, cancellationToken);
        return Ok(ApiResponse<MedicoDetalleDto>.Ok(actualizado, "Especialidades médicas actualizadas exitosamente."));
    }
}
