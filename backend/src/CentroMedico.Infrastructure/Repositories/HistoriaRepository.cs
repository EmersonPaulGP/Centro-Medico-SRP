using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para historias clínicas mediante Entity Framework Core.
/// </summary>
public class HistoriaRepository : IHistoriaRepository
{
    private readonly CentroMedicoDbContext _context;

    public HistoriaRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<Historia?> ObtenerPorIdConDetalleAsync(int idHistoria, CancellationToken cancellationToken = default)
    {
        return await _context.Historias
            .Include(h => h.Paciente)
            .Include(h => h.Medico)
            .Include(h => h.Turno)
            .FirstOrDefaultAsync(h => h.IdHistoria == idHistoria, cancellationToken);
    }

    public async Task<(IReadOnlyList<Historia> Items, int Total)> ObtenerPorPacienteIdAsync(
        int idPaciente,
        int? idMedico,
        DateTime? desde,
        DateTime? hasta,
        string? terminoBusqueda,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Historias
            .Include(h => h.Paciente)
            .Include(h => h.Medico)
            .Include(h => h.Turno)
            .AsNoTracking()
            .Where(h => h.IdPaciente == idPaciente);

        if (idMedico.HasValue)
        {
            query = query.Where(h => h.IdMedico == idMedico.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(h => h.FechaHistoria >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(h => h.FechaHistoria <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(terminoBusqueda))
        {
            var termino = terminoBusqueda.Trim().ToLower();
            query = query.Where(h =>
                (h.Diagnostico != null && h.Diagnostico.ToLower().Contains(termino)) ||
                h.Observacion.ToLower().Contains(termino));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(h => h.FechaHistoria)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Historia> AgregarAsync(Historia historia, CancellationToken cancellationToken = default)
    {
        await _context.Historias.AddAsync(historia, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return historia;
    }
}
