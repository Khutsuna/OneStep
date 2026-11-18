using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneStep.Application.Abstractions;
using OneStep.Infrastructure.Persistence;

namespace OneStep.Infrastructure;

/// <summary>
/// Registers everything that talks to the outside world (database, later email, files, JWT).
/// Program.cs calls builder.Services.AddInfrastructure(builder.Configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        // When a service asks for IAppDbContext, give it the AppDbContext of the current request.
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
