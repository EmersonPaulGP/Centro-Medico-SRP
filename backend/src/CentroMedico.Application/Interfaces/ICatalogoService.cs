using CentroMedico.Application.DTOs.Catalogos;

namespace CentroMedico.Application.Interfaces;

/// <summary>
/// Contrato para el servicio de catálogos y tablas maestras del sistema.
/// </summary>
public interface ICatalogoService
{
    Task<IEnumerable<PaisDto>> ObtenerPaisesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EspecialidadDto>> ObtenerEspecialidadesAsync(bool soloActivas = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogoGenericoDto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CatalogoGenericoDto>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default);
    Task<EspecialidadDto> CrearEspecialidadAsync(CrearEspecialidadDto dto, CancellationToken cancellationToken = default);
}
