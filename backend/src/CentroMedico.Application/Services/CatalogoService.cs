using CentroMedico.Application.DTOs.Catalogos;
using CentroMedico.Application.Interfaces;
using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Exceptions;
using CentroMedico.Domain.Interfaces;
using FluentValidation;

namespace CentroMedico.Application.Services;

/// <summary>
/// Servicio de aplicación para la gestión y consulta de catálogos y parámetros del sistema.
/// </summary>
public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IValidator<CrearEspecialidadDto> _crearEspecialidadValidator;

    public CatalogoService(
        ICatalogoRepository catalogoRepository,
        IValidator<CrearEspecialidadDto> crearEspecialidadValidator)
    {
        _catalogoRepository = catalogoRepository;
        _crearEspecialidadValidator = crearEspecialidadValidator;
    }

    public async Task<IEnumerable<PaisDto>> ObtenerPaisesAsync(CancellationToken cancellationToken = default)
    {
        var paises = await _catalogoRepository.ObtenerPaisesAsync(cancellationToken);
        return paises.Select(p => new PaisDto(p.IdPais.Trim(), p.Nombre));
    }

    public async Task<IEnumerable<EspecialidadDto>> ObtenerEspecialidadesAsync(bool soloActivas = true, CancellationToken cancellationToken = default)
    {
        bool? filtroActivo = soloActivas ? true : null;
        var especialidades = await _catalogoRepository.ObtenerEspecialidadesAsync(filtroActivo, cancellationToken);
        return especialidades.Select(e => new EspecialidadDto(e.IdEspecialidad, e.Nombre, e.Descripcion, e.Activo));
    }

    public async Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default)
    {
        var estados = await _catalogoRepository.ObtenerEstadosTurnoAsync(cancellationToken);
        return estados.Select(t => new CatalogoGenericoDto(t.IdEstado, t.Descripcion));
    }

    public async Task<IEnumerable<CatalogoGenericoDto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default)
    {
        var conceptos = await _catalogoRepository.ObtenerConceptosPagoAsync(cancellationToken);
        return conceptos.Select(c => new CatalogoGenericoDto(c.IdConcepto, c.Descripcion));
    }

    public async Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default)
    {
        var estados = await _catalogoRepository.ObtenerEstadosPagoAsync(cancellationToken);
        return estados.Select(p => new CatalogoGenericoDto(p.IdEstado, p.Descripcion));
    }

    public async Task<EspecialidadDto> CrearEspecialidadAsync(CrearEspecialidadDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validación de campos y reglas de formato
        var validationResult = await _crearEspecialidadValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // 2. Regla de negocio: unicidad de nombre de especialidad médica
        var nombreLimpio = dto.Nombre.Trim();
        var existe = await _catalogoRepository.ExisteEspecialidadConNombreAsync(nombreLimpio, cancellationToken);
        if (existe)
        {
            throw new BusinessRuleException("Ya existe una especialidad con ese nombre");
        }

        // 3. Creación y persistencia
        var nuevaEspecialidad = new Especialidad
        {
            Nombre = nombreLimpio,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Activo = true
        };

        var especialidadCreada = await _catalogoRepository.CrearEspecialidadAsync(nuevaEspecialidad, cancellationToken);

        return new EspecialidadDto(
            especialidadCreada.IdEspecialidad,
            especialidadCreada.Nombre,
            especialidadCreada.Descripcion,
            especialidadCreada.Activo
        );
    }
}
