using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Infrastructure.Auth;
using OneStep.Infrastructure.Email;
using OneStep.Infrastructure.Persistence;
using OneStep.Infrastructure.Persistence.Queries;
using OneStep.Infrastructure.Storage;

namespace OneStep.Infrastructure;

/// <summary>
/// Registers everything that talks to the outside world: the database and authentication
/// (later also email and file storage).
/// Program.cs calls builder.Services.AddInfrastructure(builder.Configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ---------- Database ----------
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default"))
                // EF warns that Product's soft-delete filter can hide a product from rows that point to it
                // (order lines, reviews...). That is intended: deleted products disappear from the shop.
                .ConfigureWarnings(warnings => warnings.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        // When a service asks for IAppDbContext, give it the AppDbContext of the current request.
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Sales reports run as stored procedures (database/04_procedures.sql).
        services.AddScoped<IAnalyticsQueries, SqlAnalyticsQueries>();

        // ---------- File storage (product images) ----------
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        // ---------- Email (password reset links) ----------
        // Development: the email is written to the log. Swap this line for a real sender in production.
        services.AddSingleton<IEmailSender, LogEmailSender>();

        // ---------- Authentication (who are you?) ----------
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing or shorter than 32 characters. Set it with: " +
                "dotnet user-secrets set \"Jwt:Key\" \"<your key>\" --project src/OneStep.Api");
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names exactly as they are in the token ("sub", "role"),
                // instead of .NET's long default names. Easier to read and explain.
                options.MapInboundClaims = false;

                // Every request with a token is checked against these rules.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,                       // expired tokens are rejected
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidateIssuerSigningKey = true,               // tampered tokens are rejected
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        // ---------- Authorization (what are you allowed to do?) ----------
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CustomerOnly, policy => policy.RequireRole("Customer"))
            .AddPolicy(Policies.StaffOnly, policy => policy.RequireRole("Admin", "StoreManager"))
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole("Admin"));

        return services;
    }
}
