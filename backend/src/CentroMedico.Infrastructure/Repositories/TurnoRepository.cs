using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Enums;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para Turnos mediante Entity Framework Core.
/// </summary>
public class TurnoRepository : ITurnoRepository
{
    private readonly CentroMedicoDbContext _context;

    public TurnoRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteConflictoHorarioAsync(
        int idMedico,
        DateTime fechaTurno,
        int? ignorarIdTurno = null,
        CancellationToken cancellationToken = default)
    {
        const short cancelado = (short)EstadoTurnoEnum.Cancelado; // 2
        const short anulado = (short)EstadoTurnoEnum.Anulado;     // 5

        return await _context.Turnos
            .AnyAsync(t => t.IdMedico == idMedico &&
                           t.FechaTurno == fechaTurno &&
                           t.IdEstado != cancelado &&
                           t.IdEstado != anulado &&
                           (!ignorarIdTurno.HasValue || t.IdTurno != ignorarIdTurno.Value), cancellationToken);
    }

    public async Task<Turno?> ObtenerPorIdAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        return await _context.Turnos
            .FirstOrDefaultAsync(t => t.IdTurno == idTurno, cancellationToken);
    }

    public async Task<Turno?> ObtenerPorIdConDetallesAsync(int idTurno, CancellationToken cancellationToken = default)
    {
        return await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.TurnoEstado)
            .FirstOrDefaultAsync(t => t.IdTurno == idTurno, cancellationToken);
    }

    public async Task<(IReadOnlyList<Turno> Items, int Total)> ObtenerAgendaAsync(
        int? idMedico,
        int? idPaciente,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        short? idEstado,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .Include(t => t.TurnoEstado)
            .AsNoTracking();

        if (idMedico.HasValue)
        {
            query = query.Where(t => t.IdMedico == idMedico.Value);
        }

        if (idPaciente.HasValue)
        {
            query = query.Where(t => t.IdPaciente == idPaciente.Value);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(t => t.FechaTurno >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(t => t.FechaTurno <= fechaFin.Value);
        }

        if (idEstado.HasValue)
        {
            query = query.Where(t => t.IdEstado == idEstado.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.FechaTurno)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Turno> AgregarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        await _context.Turnos.AddAsync(turno, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return turno;
    }

    public async Task ActualizarAsync(Turno turno, CancellationToken cancellationToken = default)
    {
        _context.Turnos.Update(turno);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
