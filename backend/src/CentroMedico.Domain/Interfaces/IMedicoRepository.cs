using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para la persistencia y consultas de la entidad Médico y sus especialidades.
/// </summary>
public interface IMedicoRepository
{
    Task<bool> ExisteMatriculaAsync(string matricula, int? ignorarIdMedico = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteDniAsync(string dni, int? ignorarIdMedico = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Medico> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        int? idEspecialidad,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default);
    Task<Medico?> ObtenerConEspecialidadesPorIdAsync(int idMedico, CancellationToken cancellationToken = default);
    Task<Medico> CrearAsync(Medico medico, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Medico medico, CancellationToken cancellationToken = default);
    Task ActualizarEspecialidadesAsync(int idMedico, IEnumerable<MedicoEspecialidad> nuevasEspecialidades, CancellationToken cancellationToken = default);
}
