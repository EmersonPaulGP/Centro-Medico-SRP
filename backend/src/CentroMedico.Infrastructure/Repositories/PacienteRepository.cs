using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para Pacientes mediante Entity Framework Core.
/// </summary>
public class PacienteRepository : IPacienteRepository
{
    private readonly CentroMedicoDbContext _context;

    public PacienteRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteDniAsync(string dni, int? ignorarIdPaciente = null, CancellationToken cancellationToken = default)
    {
        var dniLimpio = dni.Trim();
        return await _context.Pacientes
            .AnyAsync(p => p.Dni == dniLimpio &&
                           (!ignorarIdPaciente.HasValue || p.IdPaciente != ignorarIdPaciente.Value), cancellationToken);
    }

    public async Task<Paciente?> ObtenerPorIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        return await _context.Pacientes
            .Include(p => p.Pais)
            .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente, cancellationToken);
    }

    public async Task<Paciente?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var dniLimpio = dni.Trim();
        return await _context.Pacientes
            .Include(p => p.Pais)
            .FirstOrDefaultAsync(p => p.Dni == dniLimpio, cancellationToken);
    }

    public async Task<(IReadOnlyList<Paciente> Items, int Total)> ObtenerFiltradosAsync(
        string? busqueda,
        string? idPais,
        bool? soloActivos,
        int pagina,
        int registrosPorPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pacientes
            .Include(p => p.Pais)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(texto) ||
                p.Apellido.ToLower().Contains(texto) ||
                p.Dni.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(idPais))
        {
            var codigoPais = idPais.Trim().ToUpper();
            query = query.Where(p => p.IdPais == codigoPais);
        }

        if (soloActivos.HasValue)
        {
            query = query.Where(p => p.Activo == soloActivos.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Skip((pagina - 1) * registrosPorPagina)
            .Take(registrosPorPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Paciente> AgregarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return paciente;
    }

    public async Task ActualizarAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        _context.Pacientes.Update(paciente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
