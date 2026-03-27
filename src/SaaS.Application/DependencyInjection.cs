using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Application.Common.Behaviors;
using SaaS.Application.Features.Tenants.Services;
using SaaS.Application.Interfaces;
using System.Reflection;

namespace SaaS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();

        return services;
    }
}
