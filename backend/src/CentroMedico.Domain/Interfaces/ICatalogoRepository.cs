using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para el acceso y persistencia de tablas catálogo o maestras del sistema.
/// </summary>
public interface ICatalogoRepository
{
    Task<IReadOnlyList<Pais>> ObtenerPaisesAsync(CancellationToken cancellationToken = default);
    Task<Pais?> ObtenerPaisPorIdAsync(string idPais, CancellationToken cancellationToken = default);
    Task<bool> ExistePaisAsync(string idPais, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Especialidad>> ObtenerEspecialidadesAsync(bool? soloActivas = null, CancellationToken cancellationToken = default);
    Task<Especialidad?> ObtenerEspecialidadPorIdAsync(int idEspecialidad, CancellationToken cancellationToken = default);
    Task<bool> ExisteEspecialidadConNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task<Especialidad> CrearEspecialidadAsync(Especialidad especialidad, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TurnoEstado>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Concepto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default);
    Task<Concepto?> ObtenerConceptoPorIdAsync(byte idConcepto, CancellationToken cancellationToken = default);
    Task<bool> ExisteConceptoAsync(byte idConcepto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PagoEstado>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default);
}
