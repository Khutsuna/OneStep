using Microsoft.Extensions.DependencyInjection;
using OneStep.Application.Features.DeliveryMethods;
using OneStep.Application.Features.Auth;
using OneStep.Application.Features.Users;
using OneStep.Application.Features.Attributes;
using OneStep.Application.Features.Brands;
using OneStep.Application.Features.Categories;
using OneStep.Application.Features.Pricing;
using OneStep.Application.Features.Products;
using OneStep.Application.Features.Addresses;
using OneStep.Application.Features.Cart;
using OneStep.Application.Features.Orders;
using OneStep.Application.Features.Analytics;
using OneStep.Application.Features.Campaigns;
using OneStep.Application.Features.Notifications;


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
        services.AddScoped<IUserService, UserService>();

        // Catalog
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IAttributeService, AttributeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductImageService, ProductImageService>();

        // Shopping
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IOrderService, OrderService>();

        // Admin panel
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICampaignService, CampaignService>();

        return services;
    }
}
