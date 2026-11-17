using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para catálogos del sistema mediante EF Core.
/// </summary>
public class CatalogoRepository : ICatalogoRepository
{
    private readonly CentroMedicoDbContext _context;

    public CatalogoRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pais>> ObtenerPaisesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Paises
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pais?> ObtenerPaisPorIdAsync(string idPais, CancellationToken cancellationToken = default)
    {
        var codigo = idPais.Trim().ToUpper();
        return await _context.Paises
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPais == codigo, cancellationToken);
    }

    public async Task<bool> ExistePaisAsync(string idPais, CancellationToken cancellationToken = default)
    {
        var codigo = idPais.Trim().ToUpper();
        return await _context.Paises
            .AnyAsync(p => p.IdPais == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<Especialidad>> ObtenerEspecialidadesAsync(bool? soloActivas = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Especialidades.AsNoTracking();

        if (soloActivas.HasValue)
        {
            query = query.Where(e => e.Activo == soloActivas.Value);
        }

        return await query
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Especialidad?> ObtenerEspecialidadPorIdAsync(int idEspecialidad, CancellationToken cancellationToken = default)
    {
        return await _context.Especialidades
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEspecialidad == idEspecialidad, cancellationToken);
    }

    public async Task<bool> ExisteEspecialidadConNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre.Trim().ToLower();
        return await _context.Especialidades
            .AnyAsync(e => e.Nombre.ToLower() == nombreNormalizado, cancellationToken);
    }

    public async Task<Especialidad> CrearEspecialidadAsync(Especialidad especialidad, CancellationToken cancellationToken = default)
    {
        await _context.Especialidades.AddAsync(especialidad, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return especialidad;
    }

    public async Task<IReadOnlyList<TurnoEstado>> ObtenerEstadosTurnoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TurnosEstados
            .AsNoTracking()
            .OrderBy(t => t.IdEstado)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Concepto>> ObtenerConceptosPagoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Conceptos
            .AsNoTracking()
            .OrderBy(c => c.IdConcepto)
            .ToListAsync(cancellationToken);
    }

    public async Task<Concepto?> ObtenerConceptoPorIdAsync(byte idConcepto, CancellationToken cancellationToken = default)
    {
        return await _context.Conceptos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdConcepto == idConcepto, cancellationToken);
    }

    public async Task<bool> ExisteConceptoAsync(byte idConcepto, CancellationToken cancellationToken = default)
    {
        return await _context.Conceptos
            .AnyAsync(c => c.IdConcepto == idConcepto, cancellationToken);
    }

    public async Task<IReadOnlyList<PagoEstado>> ObtenerEstadosPagoAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PagosEstados
            .AsNoTracking()
            .OrderBy(p => p.IdEstado)
            .ToListAsync(cancellationToken);
    }
}
