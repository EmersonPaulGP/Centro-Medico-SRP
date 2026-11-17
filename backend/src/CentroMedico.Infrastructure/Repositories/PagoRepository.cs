using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para pagos y transacciones de cobro mediante EF Core.
/// </summary>
public class PagoRepository : IPagoRepository
{
    private readonly CentroMedicoDbContext _context;

    public PagoRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<Pago?> ObtenerPorIdAsync(int idPago, CancellationToken cancellationToken = default)
    {
        return await _context.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPago == idPago, cancellationToken);
    }

    public async Task<Pago?> ObtenerPorIdConDetalleAsync(int idPago, CancellationToken cancellationToken = default)
    {
        return await _context.Pagos
            .Include(p => p.Paciente)
            .Include(p => p.Concepto)
            .Include(p => p.PagoEstado)
            .Include(p => p.Turno)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPago == idPago, cancellationToken);
    }

    public async Task<(IReadOnlyList<Pago> Items, int Total)> ObtenerFiltradosAsync(
        int? idPaciente,
        byte? idConcepto,
        byte? idEstado,
        DateTime? desde,
        DateTime? hasta,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pagos
            .Include(p => p.Paciente)
            .Include(p => p.Concepto)
            .Include(p => p.PagoEstado)
            .Include(p => p.Turno)
            .AsNoTracking()
            .AsQueryable();

        if (idPaciente.HasValue)
        {
            query = query.Where(p => p.IdPaciente == idPaciente.Value);
        }

        if (idConcepto.HasValue)
        {
            query = query.Where(p => p.IdConcepto == idConcepto.Value);
        }

        if (idEstado.HasValue)
        {
            query = query.Where(p => p.IdEstado == idEstado.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(p => p.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(p => p.Fecha <= hasta.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.IdPago)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Pago> AgregarAsync(Pago pago, CancellationToken cancellationToken = default)
    {
        await _context.Pagos.AddAsync(pago, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return pago;
    }
}
