using Microsoft.EntityFrameworkCore;

namespace CentroMedico.Infrastructure.Context;

public class CentroMedicoDbContext : DbContext
{
    public CentroMedicoDbContext(DbContextOptions<CentroMedicoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CentroMedicoDbContext).Assembly);
    }
}
