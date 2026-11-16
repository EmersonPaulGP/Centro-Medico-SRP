using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CentroMedico.Api.Controllers.v1;

/// <summary>
/// Endpoints RESTful para la administración clínica y datos de pacientes.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;
    private readonly IPacienteInfoService _pacienteInfoService;

    public PacientesController(
        IPacienteService pacienteService,
        IPacienteInfoService pacienteInfoService)
    {
        _pacienteService = pacienteService;
        _pacienteInfoService = pacienteInfoService;
    }

    public PacientesController(IPacienteService pacienteService)
        : this(pacienteService, null!)
    {
    }

    /// <summary>
    /// Consulta el listado paginado de pacientes con filtros de búsqueda por nombre, apellido, DNI, país y estado activo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PacienteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPacientes([FromQuery] PacienteFiltroDto filtro, CancellationToken cancellationToken)
    {
        var resultado = await _pacienteService.ListarAsync(filtro, cancellationToken);
        return Ok(ApiResponse<PagedResult<PacienteDto>>.Ok(resultado, "Listado de pacientes obtenido exitosamente."));
    }

    /// <summary>
    /// Obtiene los datos completos de un paciente a partir de su identificador único.
    /// </summary>
    [HttpGet("{id:int}", Name = nameof(ObtenerPacientePorId))]
    [ProducesResponseType(typeof(ApiResponse<PacienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPacientePorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var paciente = await _pacienteService.ObtenerPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PacienteDto>.Ok(paciente, "Paciente encontrado exitosamente."));
    }

    /// <summary>
    /// Obtiene el perfil de un paciente a partir de su documento de identidad (DNI).
    /// </summary>
    [HttpGet("dni/{dni}")]
    [ProducesResponseType(typeof(ApiResponse<PacienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPacientePorDni([FromRoute] string dni, CancellationToken cancellationToken)
    {
        var paciente = await _pacienteService.ObtenerPorDniAsync(dni, cancellationToken);
        return Ok(ApiResponse<PacienteDto>.Ok(paciente, "Paciente encontrado exitosamente."));
    }

    /// <summary>
    /// Registra un nuevo paciente en el sistema validando unicidad de DNI y existencia de país.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PacienteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CrearPaciente([FromBody] CrearPacienteDto dto, CancellationToken cancellationToken)
    {
        var creado = await _pacienteService.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPacientePorId),
            new { id = creado.IdPaciente },
            ApiResponse<PacienteDto>.Ok(creado, "Paciente registrado exitosamente.")
        );
    }

    /// <summary>
    /// Actualiza la información personal, de contacto y domicilio de un paciente existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PacienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ActualizarPaciente(
        [FromRoute] int id,
        [FromBody] ActualizarPacienteDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _pacienteService.ActualizarAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PacienteDto>.Ok(actualizado, "Datos del paciente actualizados exitosamente."));
    }

    /// <summary>
    /// Cambia el estado activo (baja lógica o reactivación) de un paciente.
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        [FromRoute] int id,
        [FromBody] CambiarEstadoPacienteDto dto,
        CancellationToken cancellationToken)
    {
        await _pacienteService.CambiarEstadoActivoAsync(id, dto.Activo, cancellationToken);
        var mensaje = dto.Activo
            ? "El paciente ha sido reactivado exitosamente."
            : "Baja lógica realizada exitosamente. El paciente ya no estará disponible para nuevas consultas.";

        return Ok(ApiResponse<object>.Ok(new { idPaciente = id, activo = dto.Activo }, mensaje));
    }

    /// <summary>
    /// Realiza la baja lógica (activo = false) de un paciente según la convención RESTful DELETE.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja([FromRoute] int id, CancellationToken cancellationToken)
    {
        await _pacienteService.BajaLogicaAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Consulta los antecedentes clínicos (ficha médica de base) de un paciente.
    /// Si el paciente no posee antecedentes cargados, retorna valores por defecto (false / null).
    /// </summary>
    [HttpGet("{idPaciente:int}/antecedentes")]
    [ProducesResponseType(typeof(ApiResponse<PacienteInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerAntecedentes(
        [FromRoute] int idPaciente,
        CancellationToken cancellationToken)
    {
        var antecedentes = await _pacienteInfoService.ObtenerPorPacienteIdAsync(idPaciente, cancellationToken);
        return Ok(ApiResponse<PacienteInfoDto>.Ok(antecedentes, "Antecedentes clínicos obtenidos exitosamente."));
    }

    /// <summary>
    /// Registra o actualiza (Upsert) los antecedentes clínicos del paciente.
    /// Valida existencia del paciente y formato del grupo sanguíneo.
    /// </summary>
    [HttpPut("{idPaciente:int}/antecedentes")]
    [ProducesResponseType(typeof(ApiResponse<PacienteInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GuardarAntecedentes(
        [FromRoute] int idPaciente,
        [FromBody] GuardarPacienteInfoDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _pacienteInfoService.GuardarOActualizarAsync(idPaciente, dto, cancellationToken);
        return Ok(ApiResponse<PacienteInfoDto>.Ok(resultado, "Antecedentes clínicos guardados exitosamente."));
    }
}
