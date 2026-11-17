using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pagos;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la gestión y facturación de cobros a pacientes.
/// </summary>
public class PagoService : IPagoService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly IValidator<RegistrarPagoDto> _validator;

    public PagoService(
        IPagoRepository pagoRepository,
        IPacienteRepository pacienteRepository,
        ICatalogoRepository catalogoRepository,
        ITurnoRepository turnoRepository,
        IValidator<RegistrarPagoDto> validator)
    {
        _pagoRepository = pagoRepository;
        _pacienteRepository = pacienteRepository;
        _catalogoRepository = catalogoRepository;
        _turnoRepository = turnoRepository;
        _validator = validator;
    }

    public async Task<PagoDetalleDto> RegistrarPagoAsync(RegistrarPagoDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de formato de entrada
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMonto = validationResult.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.Monto));
            if (errorMonto != null)
            {
                throw new BusinessRuleException(errorMonto.ErrorMessage);
            }

            var errorMetodo = validationResult.Errors.FirstOrDefault(e => e.PropertyName == nameof(dto.MetodoPago));
            if (errorMetodo != null)
            {
                throw new BusinessRuleException(errorMetodo.ErrorMessage);
            }

            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        if (dto.Monto <= 0)
        {
            throw new BusinessRuleException("El monto a cobrar debe ser estrictamente mayor a cero");
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

        // 3. Verificar existencia del concepto de pago
        var concepto = await _catalogoRepository.ObtenerConceptoPorIdAsync(dto.IdConcepto, cancellationToken);
        if (concepto == null)
        {
            throw new NotFoundException($"El concepto de pago con ID '{dto.IdConcepto}' no existe en el catálogo.");
        }

        // 4. Validar turno asociado si fue provisto
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

            if (turno.IdEstado == (short)EstadoTurnoEnum.Cancelado)
            {
                throw new BusinessRuleException("No es posible asociar un cobro a un turno cancelado");
            }

            if (turno.IdEstado == (short)EstadoTurnoEnum.Anulado)
            {
                throw new BusinessRuleException("No es posible asociar un cobro a un turno anulado");
            }
        }

        // 5. Persistencia del cobro con estado Aprobado (1)
        var fechaActual = DateTime.UtcNow;
        var pago = new Pago
        {
            IdPaciente = dto.IdPaciente,
            IdTurno = dto.IdTurno,
            IdConcepto = dto.IdConcepto,
            Fecha = fechaActual,
            Monto = dto.Monto,
            IdEstado = (byte)EstadoPagoEnum.Aprobado,
            MetodoPago = dto.MetodoPago?.Trim(),
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
            FechaAlta = fechaActual,
            Paciente = paciente,
            Concepto = concepto,
            Turno = turno
        };

        var creado = await _pagoRepository.AgregarAsync(pago, cancellationToken);

        var detalle = await _pagoRepository.ObtenerPorIdConDetalleAsync(creado.IdPago, cancellationToken)
            ?? creado;

        return MapearAPagoDetalleDto(detalle);
    }

    public async Task<PagoDetalleDto> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdConDetalleAsync(idPago, cancellationToken);
        if (pago == null)
        {
            throw new NotFoundException($"No se encontró ningún pago con el ID '{idPago}'");
        }

        return MapearAPagoDetalleDto(pago);
    }

    public async Task<ComprobantePagoDto> ObtenerComprobanteAsync(int idPago, CancellationToken cancellationToken = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdConDetalleAsync(idPago, cancellationToken);
        if (pago == null)
        {
            throw new NotFoundException($"No se encontró ningún comprobante para el cobro con ID '{idPago}'");
        }

        var pacienteNombre = pago.Paciente != null
            ? $"{pago.Paciente.Apellido}, {pago.Paciente.Nombre}"
            : "Paciente Desconocido";

        var pacienteDni = pago.Paciente?.Dni ?? string.Empty;
        var pacienteDomicilio = pago.Paciente?.Domicilio ?? string.Empty;
        var conceptoDesc = pago.Concepto?.Descripcion ?? "Consulta Médica General";
        var metodo = pago.MetodoPago ?? "Efectivo";

        return new ComprobantePagoDto(
            NumeroComprobante: pago.IdPago,
            FechaEmision: pago.Fecha,
            CentroMedicoNombre: "Centro Médico SRP",
            CentroMedicoCuit: "30-71234567-9",
            CentroMedicoDireccion: "Av. Principal 1234, CABA",
            IdPaciente: pago.IdPaciente,
            PacienteNombre: pacienteNombre,
            PacienteDni: pacienteDni,
            PacienteDomicilio: pacienteDomicilio,
            IdConcepto: pago.IdConcepto,
            Concepto: conceptoDesc,
            IdTurno: pago.IdTurno,
            Subtotal: pago.Monto,
            Total: pago.Monto,
            MetodoPago: metodo,
            Observacion: pago.Observacion
        );
    }

    public async Task<PagedResult<PagoDetalleDto>> ListarPagosAsync(PagoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = filtro.Pagina > 0 ? filtro.Pagina : 1;
        var registrosPorPagina = filtro.RegistrosPorPagina > 0 ? Math.Min(filtro.RegistrosPorPagina, 100) : 20;

        var (items, total) = await _pagoRepository.ObtenerFiltradosAsync(
            filtro.IdPaciente,
            filtro.IdConcepto,
            filtro.IdEstado,
            filtro.Desde,
            filtro.Hasta,
            pagina,
            registrosPorPagina,
            cancellationToken);

        var dtos = items.Select(MapearAPagoDetalleDto).ToList();
        return new PagedResult<PagoDetalleDto>(dtos, total, pagina, registrosPorPagina);
    }

    private static PagoDetalleDto MapearAPagoDetalleDto(Pago p)
    {
        var pacienteNombreCompleto = p.Paciente != null
            ? $"{p.Paciente.Apellido}, {p.Paciente.Nombre}"
            : string.Empty;

        var conceptoDesc = p.Concepto != null
            ? p.Concepto.Descripcion
            : string.Empty;

        var estadoDesc = p.PagoEstado != null
            ? p.PagoEstado.Descripcion
            : ((EstadoPagoEnum)p.IdEstado).ToString();

        return new PagoDetalleDto(
            p.IdPago,
            p.Fecha,
            p.IdPaciente,
            pacienteNombreCompleto,
            p.Paciente?.Dni ?? string.Empty,
            p.IdTurno,
            p.IdConcepto,
            conceptoDesc,
            p.Monto,
            p.IdEstado,
            estadoDesc,
            p.MetodoPago,
            p.Observacion
        );
    }
}
