using CentroMedico.Domain.Entities;

namespace CentroMedico.Domain.Interfaces;

/// <summary>
/// Contrato de persistencia para la gestión y agendamiento de turnos médicos.
/// </summary>
public interface ITurnoRepository
{
    Task<bool> ExisteConflictoHorarioAsync(int idMedico, DateTime fechaTurno, int? ignorarIdTurno = null, CancellationToken cancellationToken = default);
    Task<Turno?> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default);
    Task<Turno?> ObtenerPorIdConDetallesAsync(int idTurno, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Turno> Items, int Total)> ObtenerAgendaAsync(
        int? idMedico,
        int? idPaciente,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        short? idEstado,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default);
    Task<Turno> AgregarAsync(Turno turno, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default);
}
