using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Context;

public class CentroMedicoDbContext : DbContext
{
    public CentroMedicoDbContext(DbContextOptions<CentroMedicoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<TurnoEstado> TurnosEstados => Set<TurnoEstado>();
    public DbSet<Concepto> Conceptos => Set<Concepto>();
    public DbSet<PagoEstado> PagosEstados => Set<PagoEstado>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<MedicoEspecialidad> MedicosEspecialidades => Set<MedicoEspecialidad>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<PacienteInfo> PacientesInfo => Set<PacienteInfo>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CentroMedicoDbContext).Assembly);
    }
}
