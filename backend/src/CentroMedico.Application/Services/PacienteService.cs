using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Pacientes;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la administración y consulta de pacientes.
/// </summary>
public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IValidator<CrearPacienteDto> _crearValidator;
    private readonly IValidator<ActualizarPacienteDto> _actualizarValidator;

    public PacienteService(
        IPacienteRepository pacienteRepository,
        ICatalogoRepository catalogoRepository,
        IValidator<CrearPacienteDto> crearValidator,
        IValidator<ActualizarPacienteDto> actualizarValidator)
    {
        _pacienteRepository = pacienteRepository;
        _catalogoRepository = catalogoRepository;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
    }

    public async Task<PacienteDto> CrearAsync(CrearPacienteDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de formato y campos requeridos
        var validationResult = await _crearValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Regla de negocio: unicidad de DNI
        if (await _pacienteRepository.ExisteDniAsync(dto.Dni, null, cancellationToken))
        {
            throw new BusinessRuleException("Ya existe un paciente registrado con el DNI proporcionado");
        }

        // 3. Regla de negocio: existencia del país en catálogo
        var codigoPais = dto.IdPais.Trim().ToUpper();
        var pais = await _catalogoRepository.ObtenerPaisPorIdAsync(codigoPais, cancellationToken);
        if (pais == null)
        {
            throw new BusinessRuleException($"El país con código '{dto.IdPais}' no existe en el catálogo");
        }

        // 4. Mapeo y persistencia
        var paciente = new Paciente
        {
            Dni = dto.Dni.Trim(),
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            FNacimiento = dto.FNacimiento,
            Domicilio = dto.Domicilio.Trim(),
            IdPais = codigoPais,
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
            Activo = true,
            FechaAlta = DateTime.UtcNow,
            Pais = pais
        };

        var creado = await _pacienteRepository.AgregarAsync(paciente, cancellationToken);

        var resultado = await _pacienteRepository.ObtenerPorIdAsync(creado.IdPaciente, cancellationToken)
            ?? creado;

        return MapearAPacienteDto(resultado);
    }

    public async Task<PacienteDto> ActualizarAsync(int idPaciente, ActualizarPacienteDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de formato
        var validationResult = await _actualizarValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Existencia de paciente
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken)
            ?? throw new NotFoundException("Paciente", idPaciente);

        // 3. Existencia del país
        var codigoPais = dto.IdPais.Trim().ToUpper();
        var pais = await _catalogoRepository.ObtenerPaisPorIdAsync(codigoPais, cancellationToken);
        if (pais == null)
        {
            throw new BusinessRuleException($"El país con código '{dto.IdPais}' no existe en el catálogo");
        }

        // 4. Actualización de datos permitidos
        paciente.Nombre = dto.Nombre.Trim();
        paciente.Apellido = dto.Apellido.Trim();
        paciente.FNacimiento = dto.FNacimiento;
        paciente.Domicilio = dto.Domicilio.Trim();
        paciente.IdPais = codigoPais;
        paciente.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        paciente.Email = dto.Email.Trim().ToLower();
        paciente.Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();
        paciente.FechaModificacion = DateTime.UtcNow;
        paciente.Pais = pais;

        await _pacienteRepository.ActualizarAsync(paciente, cancellationToken);

        return MapearAPacienteDto(paciente);
    }

    public async Task<PacienteDto> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken)
            ?? throw new NotFoundException("Paciente", idPaciente);

        return MapearAPacienteDto(paciente);
    }

    public async Task<PacienteDto> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.ObtenerPorDniAsync(dni, cancellationToken)
            ?? throw new NotFoundException($"No se encontró ningún paciente con el DNI '{dni}'.");

        return MapearAPacienteDto(paciente);
    }

    public async Task<PagedResult<PacienteDto>> ListarAsync(PacienteFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var registrosPorPagina = filtro.RegistrosPorPagina switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => filtro.RegistrosPorPagina
        };

        var (items, total) = await _pacienteRepository.ObtenerFiltradosAsync(
            filtro.Busqueda,
            filtro.IdPais,
            filtro.SoloActivos,
            pagina,
            registrosPorPagina,
            cancellationToken);

        var dtos = items.Select(MapearAPacienteDto).ToList();

        return new PagedResult<PacienteDto>(dtos, total, pagina, registrosPorPagina);
    }

    public async Task BajaLogicaAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        await CambiarEstadoActivoAsync(idPaciente, false, cancellationToken);
    }

    public async Task CambiarEstadoActivoAsync(int idPaciente, bool activo, CancellationToken cancellationToken = default)
    {
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente, cancellationToken)
            ?? throw new NotFoundException("Paciente", idPaciente);

        paciente.Activo = activo;
        paciente.FechaModificacion = DateTime.UtcNow;

        await _pacienteRepository.ActualizarAsync(paciente, cancellationToken);
    }

    private static PacienteDto MapearAPacienteDto(Paciente paciente)
    {
        return new PacienteDto(
            paciente.IdPaciente,
            paciente.Dni,
            paciente.Nombre,
            paciente.Apellido,
            paciente.FNacimiento,
            paciente.Edad,
            paciente.Domicilio,
            paciente.IdPais,
            paciente.Pais?.Nombre ?? string.Empty,
            paciente.Telefono,
            paciente.Email,
            paciente.Observacion,
            paciente.Activo,
            paciente.FechaAlta,
            paciente.FechaModificacion
        );
    }
}
