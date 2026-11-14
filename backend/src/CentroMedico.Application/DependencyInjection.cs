using System.Reflection;
using CentroMedico.Application.Interfaces;
using CentroMedico.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CentroMedico.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IMedicoService, MedicoService>();

        return services;
    }
}
