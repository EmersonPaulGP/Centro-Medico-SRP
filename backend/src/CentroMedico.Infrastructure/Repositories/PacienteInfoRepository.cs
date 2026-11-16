using CentroMedico.Domain.Entities;
using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para PacienteInfo mediante Entity Framework Core.
/// </summary>
public class PacienteInfoRepository : IPacienteInfoRepository
{
    private readonly CentroMedicoDbContext _context;

    public PacienteInfoRepository(CentroMedicoDbContext context)
    {
        _context = context;
    }

    public async Task<PacienteInfo?> ObtenerPorPacienteIdAsync(int idPaciente, CancellationToken cancellationToken = default)
    {
        return await _context.PacientesInfo
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.IdPaciente == idPaciente, cancellationToken);
    }

    public async Task GuardarOActualizarAsync(PacienteInfo info, CancellationToken cancellationToken = default)
    {
        var existente = await _context.PacientesInfo
            .FirstOrDefaultAsync(pi => pi.IdPaciente == info.IdPaciente, cancellationToken);

        if (existente == null)
        {
            await _context.PacientesInfo.AddAsync(info, cancellationToken);
        }
        else
        {
            existente.Diabetico = info.Diabetico;
            existente.Implantes = info.Implantes;
            existente.Alergias = info.Alergias;
            existente.GrupoSanguineo = info.GrupoSanguineo;
            existente.FechaModificacion = info.FechaModificacion;
            _context.PacientesInfo.Update(existente);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
