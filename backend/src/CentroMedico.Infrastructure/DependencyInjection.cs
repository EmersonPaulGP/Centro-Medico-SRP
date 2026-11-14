using CentroMedico.Infrastructure.Context;
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

        return services;
    }
}
