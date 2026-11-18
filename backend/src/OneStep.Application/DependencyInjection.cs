using Microsoft.Extensions.DependencyInjection;
using OneStep.Application.Features.DeliveryMethods;
using OneStep.Application.Features.Auth;


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
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
