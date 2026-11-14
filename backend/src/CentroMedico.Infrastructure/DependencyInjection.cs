using CentroMedico.Domain.Interfaces;
using CentroMedico.Infrastructure.Context;
using CentroMedico.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CentroMedico.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no fue encontrada.");

        services.AddDbContext<CentroMedicoDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Registro de repositorios de infraestructura
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IMedicoRepository, MedicoRepository>();

        return services;
    }
}
