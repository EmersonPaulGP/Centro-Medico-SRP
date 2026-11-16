using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Turnos;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la gestión y agendamiento de citas médicas.
/// </summary>
public class TurnoService : ITurnoService
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IMedicoRepository _medicoRepository;
    private readonly IValidator<AgendarTurnoDto> _agendarValidator;
    private readonly IValidator<CancelarTurnoDto> _cancelarValidator;
    private readonly IValidator<ActualizarEstadoTurnoDto> _actualizarEstadoValidator;

    public TurnoService(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IMedicoRepository medicoRepository,
        IValidator<AgendarTurnoDto> agendarValidator,
        IValidator<CancelarTurnoDto> cancelarValidator,
        IValidator<ActualizarEstadoTurnoDto> actualizarEstadoValidator)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _medicoRepository = medicoRepository;
        _agendarValidator = agendarValidator;
        _cancelarValidator = cancelarValidator;
        _actualizarEstadoValidator = actualizarEstadoValidator;
    }

    public async Task<TurnoDetalleDto> AgendarAsync(AgendarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de formato de entrada
        var validationResult = await _agendarValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. RF-03: Validar que el paciente exista y esté activo
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(dto.IdPaciente, cancellationToken);
        if (paciente == null)
        {
            throw new NotFoundException($"No se encontró ningún paciente con el ID '{dto.IdPaciente}'");
        }
        if (!paciente.Activo)
        {
            throw new BusinessRuleException("El paciente no se encuentra activo para agendar turnos");
        }

        // 3. RF-03: Validar que el médico exista y esté activo
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(dto.IdMedico, cancellationToken);
        if (medico == null)
        {
            throw new NotFoundException($"No se encontró ningún médico con el ID '{dto.IdMedico}'");
        }
        if (!medico.Activo)
        {
            throw new BusinessRuleException("El médico no se encuentra activo para atender turnos");
        }

        // 4. RF-02: Regla de conflicto de agenda del médico
        var existeConflicto = await _turnoRepository.ExisteConflictoHorarioAsync(
            dto.IdMedico,
            dto.FechaTurno,
            null,
            cancellationToken);

        if (existeConflicto)
        {
            throw new BusinessRuleException("El médico ya cuenta con una cita activa en ese horario");
        }

        // 5. RF-01: Registro con estado Pendiente (0)
        var turno = new Turno
        {
            IdPaciente = dto.IdPaciente,
            IdMedico = dto.IdMedico,
            FechaTurno = dto.FechaTurno,
            IdEstado = (short)EstadoTurnoEnum.Pendiente,
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
            FechaAlta = DateTime.UtcNow,
            Paciente = paciente,
            Medico = medico
        };

        var creado = await _turnoRepository.AgregarAsync(turno, cancellationToken);

        var turnoCompleto = await _turnoRepository.ObtenerPorIdConDetallesAsync(creado.IdTurno, cancellationToken)
            ?? creado;

        return MapearATurnoDetalleDto(turnoCompleto);
    }

    public async Task<TurnoDetalleDto> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        var turno = await _turnoRepository.ObtenerPorIdConDetallesAsync(idTurno, cancellationToken);
        if (turno == null)
        {
            throw new NotFoundException($"No se encontró ningún turno con el ID '{idTurno}'");
        }

        return MapearATurnoDetalleDto(turno);
    }

    public async Task<PagedResult<TurnoDetalleDto>> ObtenerAgendaAsync(TurnoFiltroAgendaDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = filtro.Pagina > 0 ? filtro.Pagina : 1;
        var registrosPorPagina = filtro.RegistrosPorPagina > 0 ? Math.Min(filtro.RegistrosPorPagina, 100) : 20;

        var (items, total) = await _turnoRepository.ObtenerAgendaAsync(
            filtro.IdMedico,
            filtro.IdPaciente,
            filtro.FechaInicio,
            filtro.FechaFin,
            filtro.IdEstado,
            pagina,
            registrosPorPagina,
            cancellationToken);

        var dtos = items.Select(MapearATurnoDetalleDto).ToList();
        return new PagedResult<TurnoDetalleDto>(dtos, total, pagina, registrosPorPagina);
    }

    public async Task<TurnoDetalleDto> CambiarEstadoAsync(int idTurno, ActualizarEstadoTurnoDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de entrada
        var validationResult = await _actualizarEstadoValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Localizar turno
        var turno = await _turnoRepository.ObtenerPorIdConDetallesAsync(idTurno, cancellationToken);
        if (turno == null)
        {
            throw new NotFoundException($"No se encontró ningún turno con el ID '{idTurno}'");
        }

        // 3. Reglas de transición de estados
        if (turno.IdEstado == (short)EstadoTurnoEnum.Realizado && dto.NuevoEstado != (short)EstadoTurnoEnum.Realizado)
        {
            throw new BusinessRuleException("No es posible modificar el estado de un turno ya realizado");
        }

        // Si se reabre a Pendiente, verificar disponibilidad horaria
        if (dto.NuevoEstado == (short)EstadoTurnoEnum.Pendiente && turno.IdEstado != (short)EstadoTurnoEnum.Pendiente)
        {
            var existeConflicto = await _turnoRepository.ExisteConflictoHorarioAsync(
                turno.IdMedico,
                turno.FechaTurno,
                turno.IdTurno,
                cancellationToken);

            if (existeConflicto)
            {
                throw new BusinessRuleException("El médico ya cuenta con una cita activa en ese horario");
            }
        }

        turno.IdEstado = dto.NuevoEstado;
        if (!string.IsNullOrWhiteSpace(dto.Observacion))
        {
            turno.Observacion = dto.Observacion.Trim();
        }
        turno.FechaModificacion = DateTime.UtcNow;

        await _turnoRepository.ActualizarAsync(turno, cancellationToken);

        return MapearATurnoDetalleDto(turno);
    }

    public async Task<TurnoDetalleDto> CancelarTurnoAsync(int idTurno, CancelarTurnoDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de motivo
        var validationResult = await _cancelarValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Localizar turno
        var turno = await _turnoRepository.ObtenerPorIdConDetallesAsync(idTurno, cancellationToken);
        if (turno == null)
        {
            throw new NotFoundException($"No se encontró ningún turno con el ID '{idTurno}'");
        }

        // 3. Reglas de cancelación
        if (turno.IdEstado == (short)EstadoTurnoEnum.Cancelado)
        {
            throw new BusinessRuleException("El turno ya se encuentra cancelado");
        }
        if (turno.IdEstado == (short)EstadoTurnoEnum.Realizado)
        {
            throw new BusinessRuleException("No es posible cancelar un turno que ya ha sido realizado");
        }
        if (turno.IdEstado == (short)EstadoTurnoEnum.Anulado)
        {
            throw new BusinessRuleException("El turno ya se encuentra anulado");
        }

        // 4. RF-07: Cancelar y liberar horario registrando motivo
        turno.IdEstado = (short)EstadoTurnoEnum.Cancelado;
        var motivo = $"Cancelado: {dto.MotivoCancelacion.Trim()}";
        turno.Observacion = string.IsNullOrWhiteSpace(turno.Observacion)
            ? motivo
            : $"{turno.Observacion} | {motivo}";

        if (turno.Observacion.Length > 1000)
        {
            turno.Observacion = turno.Observacion[..1000];
        }

        turno.FechaModificacion = DateTime.UtcNow;

        await _turnoRepository.ActualizarAsync(turno, cancellationToken);

        return MapearATurnoDetalleDto(turno);
    }

    private static TurnoDetalleDto MapearATurnoDetalleDto(Turno turno)
    {
        var pacienteNombreCompleto = turno.Paciente != null
            ? $"{turno.Paciente.Apellido}, {turno.Paciente.Nombre}"
            : string.Empty;

        var medicoNombreCompleto = turno.Medico != null
            ? $"{turno.Medico.Apellido}, {turno.Medico.Nombre}"
            : string.Empty;

        var estadoDescripcion = turno.TurnoEstado?.Descripcion
            ?? ((EstadoTurnoEnum)turno.IdEstado).ToString();

        return new TurnoDetalleDto(
            turno.IdTurno,
            turno.FechaTurno,
            turno.IdEstado,
            estadoDescripcion,
            turno.IdPaciente,
            pacienteNombreCompleto,
            turno.Paciente?.Dni ?? string.Empty,
            turno.Paciente?.Telefono,
            turno.IdMedico,
            medicoNombreCompleto,
            turno.Medico?.Matricula ?? string.Empty,
            turno.Observacion,
            turno.FechaAlta,
            turno.FechaModificacion
        );
    }
}
