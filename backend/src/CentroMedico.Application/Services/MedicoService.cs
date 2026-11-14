using CentroMedico.Application.Common.Models;
using CentroMedico.Application.DTOs.Medicos;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Implementación de casos de uso para la administración y perfiles de médicos.
/// </summary>
public class MedicoService : IMedicoService
{
    private readonly IMedicoRepository _medicoRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IValidator<CrearMedicoDto> _crearValidator;
    private readonly IValidator<ActualizarMedicoDto> _actualizarValidator;
    private readonly IValidator<AsignarEspecialidadDto> _asignarEspecialidadValidator;

    public MedicoService(
        IMedicoRepository medicoRepository,
        ICatalogoRepository catalogoRepository,
        IValidator<CrearMedicoDto> crearValidator,
        IValidator<ActualizarMedicoDto> actualizarValidator,
        IValidator<AsignarEspecialidadDto> asignarEspecialidadValidator)
    {
        _medicoRepository = medicoRepository;
        _catalogoRepository = catalogoRepository;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
        _asignarEspecialidadValidator = asignarEspecialidadValidator;
    }

    public async Task<MedicoDetalleDto> CrearAsync(CrearMedicoDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de formato y obligatoriedad
        var validationResult = await _crearValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Regla de negocio: unicidad de matrícula profesional
        if (await _medicoRepository.ExisteMatriculaAsync(dto.Matricula, null, cancellationToken))
        {
            throw new BusinessRuleException("Ya existe un médico registrado con esa matrícula profesional");
        }

        // 3. Regla de negocio: unicidad de DNI
        if (await _medicoRepository.ExisteDniAsync(dto.Dni, null, cancellationToken))
        {
            throw new BusinessRuleException("Ya existe un médico registrado con ese DNI");
        }

        // 4. Regla de negocio: verificar existencia y estado activo de las especialidades a vincular
        var especialidadesUnicas = dto.Especialidades!.DistinctBy(e => e.IdEspecialidad).ToList();
        var catalogoEspecialidades = new Dictionary<int, Especialidad>();

        foreach (var espDto in especialidadesUnicas)
        {
            var esp = await _catalogoRepository.ObtenerEspecialidadPorIdAsync(espDto.IdEspecialidad, cancellationToken);
            if (esp == null || !esp.Activo)
            {
                throw new BusinessRuleException($"La especialidad médica con ID {espDto.IdEspecialidad} no existe o no se encuentra activa");
            }
            catalogoEspecialidades[espDto.IdEspecialidad] = esp;
        }

        // 5. Creación de entidad y asignación de especialidades
        var medico = new Medico
        {
            Matricula = dto.Matricula.Trim(),
            Dni = dto.Dni.Trim(),
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower(),
            Activo = true,
            FechaAlta = DateTime.UtcNow
        };

        foreach (var esp in especialidadesUnicas)
        {
            medico.Especialidades.Add(new MedicoEspecialidad
            {
                IdEspecialidad = esp.IdEspecialidad,
                Descripcion = string.IsNullOrWhiteSpace(esp.Descripcion) ? null : esp.Descripcion.Trim(),
                Especialidad = catalogoEspecialidades[esp.IdEspecialidad]
            });
        }

        var creado = await _medicoRepository.CrearAsync(medico, cancellationToken);

        // Obtenemos el médico con datos de especialidades cargados
        var resultado = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(creado.IdMedico, cancellationToken)
            ?? creado;

        return MapearAMedicoDetalleDto(resultado);
    }

    public async Task<MedicoDetalleDto> ActualizarAsync(int idMedico, ActualizarMedicoDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _actualizarValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        medico.Nombre = dto.Nombre.Trim();
        medico.Apellido = dto.Apellido.Trim();
        medico.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        medico.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower();
        medico.FechaModificacion = DateTime.UtcNow;

        await _medicoRepository.ActualizarAsync(medico, cancellationToken);

        return MapearAMedicoDetalleDto(medico);
    }

    public async Task<PagedResult<MedicoDetalleDto>> ListarAsync(MedicoFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var registrosPorPagina = filtro.RegistrosPorPagina switch
        {
            < 1 => 10,
            > 100 => 100,
            _ => filtro.RegistrosPorPagina
        };

        var (items, total) = await _medicoRepository.ObtenerFiltradosAsync(
            filtro.Busqueda,
            filtro.IdEspecialidad,
            filtro.SoloActivos,
            pagina,
            registrosPorPagina,
            cancellationToken);

        var dtos = items.Select(MapearAMedicoDetalleDto).ToList();

        return new PagedResult<MedicoDetalleDto>(dtos, total, pagina, registrosPorPagina);
    }

    public async Task<MedicoDetalleDto> ObtenerPorIdAsync(int idMedico, CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        return MapearAMedicoDetalleDto(medico);
    }

    public async Task CambiarEstadoActivoAsync(int idMedico, bool activo, CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        medico.Activo = activo;
        medico.FechaModificacion = DateTime.UtcNow;

        await _medicoRepository.ActualizarAsync(medico, cancellationToken);
    }

    public async Task<MedicoDetalleDto> ActualizarEspecialidadesAsync(
        int idMedico,
        List<AsignarEspecialidadDto> especialidades,
        CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        if (especialidades == null || especialidades.Count == 0)
        {
            throw new BusinessRuleException("Debe asignar al menos una especialidad médica al médico");
        }

        foreach (var espDto in especialidades)
        {
            var valResult = await _asignarEspecialidadValidator.ValidateAsync(espDto, cancellationToken);
            if (!valResult.IsValid)
            {
                throw new FluentValidation.ValidationException(valResult.Errors);
            }
        }

        var especialidadesUnicas = especialidades.DistinctBy(e => e.IdEspecialidad).ToList();
        var catalogoEspecialidades = new Dictionary<int, Especialidad>();

        foreach (var espDto in especialidadesUnicas)
        {
            var esp = await _catalogoRepository.ObtenerEspecialidadPorIdAsync(espDto.IdEspecialidad, cancellationToken);
            if (esp == null || !esp.Activo)
            {
                throw new BusinessRuleException($"La especialidad médica con ID {espDto.IdEspecialidad} no existe o no se encuentra activa");
            }
            catalogoEspecialidades[espDto.IdEspecialidad] = esp;
        }

        var nuevas = especialidadesUnicas.Select(e => new MedicoEspecialidad
        {
            IdMedico = idMedico,
            IdEspecialidad = e.IdEspecialidad,
            Descripcion = string.IsNullOrWhiteSpace(e.Descripcion) ? null : e.Descripcion.Trim(),
            Especialidad = catalogoEspecialidades[e.IdEspecialidad]
        });

        await _medicoRepository.ActualizarEspecialidadesAsync(idMedico, nuevas, cancellationToken);

        medico.FechaModificacion = DateTime.UtcNow;
        await _medicoRepository.ActualizarAsync(medico, cancellationToken);

        var actualizado = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? medico;

        return MapearAMedicoDetalleDto(actualizado);
    }

    public async Task<MedicoDetalleDto> AsignarEspecialidadAsync(
        int idMedico,
        AsignarEspecialidadDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _asignarEspecialidadValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        if (medico.Especialidades.Any(e => e.IdEspecialidad == dto.IdEspecialidad))
        {
            throw new BusinessRuleException($"El médico ya tiene asignada la especialidad con ID {dto.IdEspecialidad}");
        }

        var especialidad = await _catalogoRepository.ObtenerEspecialidadPorIdAsync(dto.IdEspecialidad, cancellationToken);
        if (especialidad == null || !especialidad.Activo)
        {
            throw new BusinessRuleException($"La especialidad médica con ID {dto.IdEspecialidad} no existe o no se encuentra activa");
        }

        var nuevaRelacion = new MedicoEspecialidad
        {
            IdMedico = idMedico,
            IdEspecialidad = dto.IdEspecialidad,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Especialidad = especialidad
        };

        await _medicoRepository.AgregarEspecialidadAsync(nuevaRelacion, cancellationToken);

        medico.FechaModificacion = DateTime.UtcNow;
        await _medicoRepository.ActualizarAsync(medico, cancellationToken);

        var actualizado = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? medico;

        return MapearAMedicoDetalleDto(actualizado);
    }

    public async Task<MedicoDetalleDto> RemoverEspecialidadAsync(
        int idMedico,
        int idEspecialidad,
        CancellationToken cancellationToken = default)
    {
        var medico = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? throw new NotFoundException("Medico", idMedico);

        var relacion = medico.Especialidades.FirstOrDefault(e => e.IdEspecialidad == idEspecialidad)
            ?? throw new NotFoundException("Especialidad asignada al médico", idEspecialidad);

        if (medico.Especialidades.Count <= 1)
        {
            throw new BusinessRuleException("No es posible remover la especialidad. El médico debe conservar al menos una especialidad médica asignada.");
        }

        await _medicoRepository.EliminarEspecialidadAsync(idMedico, idEspecialidad, cancellationToken);

        medico.FechaModificacion = DateTime.UtcNow;
        await _medicoRepository.ActualizarAsync(medico, cancellationToken);

        var actualizado = await _medicoRepository.ObtenerConEspecialidadesPorIdAsync(idMedico, cancellationToken)
            ?? medico;

        return MapearAMedicoDetalleDto(actualizado);
    }

    private static MedicoDetalleDto MapearAMedicoDetalleDto(Medico medico)
    {
        var especialidadesDtos = medico.Especialidades?
            .Select(me => new EspecialidadAsignadaDto(
                me.IdEspecialidad,
                me.Especialidad?.Nombre ?? string.Empty,
                me.Descripcion
            ))
            .ToList() ?? new List<EspecialidadAsignadaDto>();

        return new MedicoDetalleDto(
            medico.IdMedico,
            medico.Matricula,
            medico.Dni,
            medico.Nombre,
            medico.Apellido,
            medico.Telefono,
            medico.Email,
            medico.Activo,
            medico.FechaAlta,
            medico.FechaModificacion,
            especialidadesDtos
        );
    }
}
