using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para el registro y consulta de historias clínicas.
/// </summary>
public interface IHistoriaRepository
{
    Task<Historia?> ObtenerPorIdConDetalleAsync(int idHistoria, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Historia> Items, int Total)> ObtenerPorPacienteIdAsync(
        int idPaciente,
        int? idMedico,
        DateTime? desde,
        DateTime? hasta,
        string? terminoBusqueda,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default);

    Task<Historia> AgregarAsync(Historia historia, CancellationToken cancellationToken = default);
}
