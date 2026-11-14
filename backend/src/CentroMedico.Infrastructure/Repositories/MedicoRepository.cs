using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Médicos mediante EF Core.
/// </summary>
public class MedicoRepository : IMedicoRepository
{
    private readonly CentroMedicoDbContext _context;

    public MedicoRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteMatriculaAsync(string matricula, int? ignorarIdMedico = null, CancellationToken cancellationToken = default)
    {
        var matriculaLimpia = matricula.Trim().ToLower();
        return await _context.Medicos
            .AnyAsync(m => m.Matricula.ToLower() == matriculaLimpia &&
                           (!ignorarIdMedico.HasValue || m.IdMedico != ignorarIdMedico.Value), cancellationToken);
    }

    public async Task<bool> ExisteDniAsync(string dni, int? ignorarIdMedico = null, CancellationToken cancellationToken = default)
    {
        var dniLimpio = dni.Trim();
        return await _context.Medicos
            .AnyAsync(m => m.Dni == dniLimpio &&
                           (!ignorarIdMedico.HasValue || m.IdMedico != ignorarIdMedico.Value), cancellationToken);
    }

    public async Task<(IReadOnlyList<Medico> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        int? idEspecialidad,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Medicos
            .Include(m => m.Especialidades)
                .ThenInclude(me => me.Especialidad)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(m =>
                m.Nombre.ToLower().Contains(texto) ||
                m.Apellido.ToLower().Contains(texto) ||
                m.Matricula.ToLower().Contains(texto) ||
                m.Dni.Contains(texto));
        }

        if (idEspecialidad.HasValue)
        {
            query = query.Where(m => m.Especialidades.Any(me => me.IdEspecialidad == idEspecialidad.Value));
        }

        if (soloActivos.HasValue)
        {
            query = query.Where(m => m.Activo == soloActivos.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Apellido)
            .ThenBy(m => m.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Medico?> ObtenerConEspecialidadesPorIdAsync(int idMedico, CancellationToken cancellationToken = default)
    {
        return await _context.Medicos
            .Include(m => m.Especialidades)
                .ThenInclude(me => me.Especialidad)
            .FirstOrDefaultAsync(m => m.IdMedico == idMedico, cancellationToken);
    }

    public async Task<Medico> CrearAsync(Medico medico, CancellationToken cancellationToken = default)
    {
        await _context.Medicos.AddAsync(medico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return medico;
    }

    public async Task ActualizarAsync(Medico medico, CancellationToken cancellationToken = default)
    {
        _context.Medicos.Update(medico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarEspecialidadesAsync(
        int idMedico,
        IEnumerable<MedicoEspecialidad> nuevasEspecialidades,
        CancellationToken cancellationToken = default)
    {
        var existentes = await _context.MedicosEspecialidades
            .Where(me => me.IdMedico == idMedico)
            .ToListAsync(cancellationToken);

        _context.MedicosEspecialidades.RemoveRange(existentes);
        await _context.MedicosEspecialidades.AddRangeAsync(nuevasEspecialidades, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
