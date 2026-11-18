using Microsoft.Extensions.DependencyInjection;
using OneStep.Application.Features.DeliveryMethods;

namespace OneStep.Application;

/// <summary>
/// Registers every Application-layer service in one place.
/// Program.cs calls builder.Services.AddApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped = one instance per HTTP request.
        services.AddScoped<IDeliveryMethodService, DeliveryMethodService>();

        return services;
    }
}
