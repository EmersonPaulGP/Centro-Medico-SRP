using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Historia;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la administración de historias clínicas y evoluciones médicas.
/// </summary>
public class HistoriaService : IHistoriaService
{
    private readonly IHistoriaRepository _historiaRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMedicoRepository _medicoRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly IValidator<CrearHistoriaDto> _validator;

    public HistoriaService(
        IHistoriaRepository historiaRepository,
        IPacienteRepository pacienteRepository,
        IMedicoRepository medicoRepository,
        ITurnoRepository turnoRepository,
        IValidator<CrearHistoriaDto> validator)
    {
        _historiaRepository = historiaRepository;
        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
        _turnoRepository = turnoRepository;
        _validator = validator;
    }

    public async Task<HistoriaDetalleDto> RegistrarEvolucionAsync(CrearHistoriaDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de reglas de formato
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Verificar existencia y estado del paciente
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(dto.IdPaciente, cancellationToken);
        if (paciente == null)
        {
            throw new NotFoundException($"No se encontró ningún paciente con el ID '{dto.IdPaciente}'");
        }
        if (!paciente.Activo)
        {
            throw new BusinessRuleException("El paciente no se encuentra activo");
        }

        // 3. Verificar existencia y estado del médico
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(dto.IdMedico, cancellationToken);
        if (medico == null)
        {
            throw new NotFoundException($"No se encontró ningún médico con el ID '{dto.IdMedico}'");
        }
        if (!medico.Activo)
        {
            throw new BusinessRuleException("El médico no se encuentra activo");
        }

        // 4. RF-02: Si se asocia un turno, validar coherencia y transicionar a 1 (Realizado)
        Turno? turno = null;
        if (dto.IdTurno.HasValue)
        {
            turno = await _turnoRepository.ObtenerPorIdAsync(dto.IdTurno.Value, cancellationToken);
            if (turno == null)
            {
                throw new NotFoundException($"No se encontró ningún turno con el ID '{dto.IdTurno.Value}'");
            }

            if (turno.IdPaciente != dto.IdPaciente)
            {
                throw new BusinessRuleException("El turno especificado no corresponde al paciente indicado");
            }

            if (turno.IdMedico != dto.IdMedico)
            {
                throw new BusinessRuleException("El turno especificado no corresponde al médico indicado");
            }

            if (turno.IdEstado == (short)EstadoTurnoEnum.Cancelado)
            {
                throw new BusinessRuleException("No es posible asociar una evolución clínica a un turno cancelado");
            }

            if (turno.IdEstado == (short)EstadoTurnoEnum.Anulado)
            {
                throw new BusinessRuleException("No es posible asociar una evolución clínica a un turno anulado");
            }

            // Cambiar automáticamente estado a Realizado (1)
            turno.IdEstado = (short)EstadoTurnoEnum.Realizado;
            turno.FechaModificacion = DateTime.UtcNow;
            await _turnoRepository.ActualizarAsync(turno, cancellationToken);
        }

        // 5. RF-01 & RF-06: Persistencia inmutable de la evolución clínica
        var fechaActual = DateTime.UtcNow;
        var historia = new Historia
        {
            IdPaciente = dto.IdPaciente,
            IdMedico = dto.IdMedico,
            IdTurno = dto.IdTurno,
            FechaHistoria = fechaActual,
            Diagnostico = string.IsNullOrWhiteSpace(dto.Diagnostico) ? null : dto.Diagnostico.Trim(),
            Observacion = dto.Observacion.Trim(),
            FechaAlta = fechaActual,
            Paciente = paciente,
            Medico = medico,
            Turno = turno
        };

        var creada = await _historiaRepository.AgregarAsync(historia, cancellationToken);

        var detalle = await _historiaRepository.ObtenerPorIdConDetalleAsync(creada.IdHistoria, cancellationToken)
            ?? creada;

        return MapearAHistoriaDetalleDto(detalle);
    }

    public async Task<HistoriaDetalleDto> ObtenerPorIdAsync(int idHistoria, CancellationToken cancellationToken = default)
    {
        var historia = await _historiaRepository.ObtenerPorIdConDetalleAsync(idHistoria, cancellationToken);
        if (historia == null)
        {
            throw new NotFoundException($"No se encontró ninguna evolución clínica con el ID '{idHistoria}'");
        }

        return MapearAHistoriaDetalleDto(historia);
    }

    public async Task<PagedResult<HistoriaDetalleDto>> ObtenerHistoriaPacienteAsync(
        int idPaciente,
        HistoriaFiltroDto filtro,
        CancellationToken cancellationToken = default)
    {
        // Verificar que el paciente exista
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken);
        if (paciente == null)
        {
            throw new NotFoundException($"No se encontró ningún paciente con el ID '{idPaciente}'");
        }

        var pagina = filtro.Pagina > 0 ? filtro.Pagina : 1;
        var registrosPorPagina = filtro.RegistrosPorPagina > 0 ? Math.Min(filtro.RegistrosPorPagina, 100) : 15;

        var (items, total) = await _historiaRepository.ObtenerPorPacienteIdAsync(
            idPaciente,
            filtro.IdMedico,
            filtro.Desde,
            filtro.Hasta,
            filtro.TerminoBusqueda,
            pagina,
            registrosPorPagina,
            cancellationToken);

        var dtos = items.Select(MapearAHistoriaDetalleDto).ToList();
        return new PagedResult<HistoriaDetalleDto>(dtos, total, pagina, registrosPorPagina);
    }

    private static HistoriaDetalleDto MapearAHistoriaDetalleDto(Historia h)
    {
        var pacienteNombreCompleto = h.Paciente != null
            ? $"{h.Paciente.Apellido}, {h.Paciente.Nombre}"
            : string.Empty;

        var medicoNombreCompleto = h.Medico != null
            ? $"{h.Medico.Apellido}, {h.Medico.Nombre}"
            : string.Empty;

        return new HistoriaDetalleDto(
            h.IdHistoria,
            h.FechaHistoria,
            h.IdPaciente,
            pacienteNombreCompleto,
            h.Paciente?.Dni ?? string.Empty,
            h.IdMedico,
            medicoNombreCompleto,
            h.Medico?.Matricula ?? string.Empty,
            h.IdTurno,
            h.Diagnostico,
            h.Observacion,
            h.FechaAlta
        );
    }
}
