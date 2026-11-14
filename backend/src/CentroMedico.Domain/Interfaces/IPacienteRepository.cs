using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para el acceso y persistencia de Pacientes.
/// </summary>
public interface IPacienteRepository
{
    Task<bool> ExisteDniAsync(string dni, int? ignorarIdPaciente = null, CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default);
    Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Paciente> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        string? idPais,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default);
    Task<Paciente> AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default);
}
