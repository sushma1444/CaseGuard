using CaseGuard.Backend.Assignment.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CaseGuard.Backend.Assignment.Extensions;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds custom authorization policies for role-based access control.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Admin policy - only users with Admin role
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireRole("Admin");
            });

            // OrganizationOwnerOrAdmin policy - users who are either Admin or Organization Owner/Admin
            options.AddPolicy("OrganizationOwnerOrAdmin", policy =>
            {
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.IsInRole("Owner") ||
                    context.User.IsInRole("OrganizationAdmin"));
            });

            // Member policy - any authenticated user who is a member
            options.AddPolicy("Member", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers application services (repositories, services, etc.)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register JWT token service
        services.AddScoped<Services.IJwtTokenService, Services.JwtTokenService>();
        
        // Register license expiration service
        services.AddScoped<Services.ILicenseExpirationService, Services.LicenseExpirationService>();
        
        // Register license renewal service
        services.AddScoped<Services.ILicenseRenewalService, Services.LicenseRenewalService>();
        
        // Register background services
        services.AddHostedService<Services.LicenseRenewalBackgroundService>();
        
        // Additional services will be registered here as they are created
        // Example: services.AddScoped<IOrganizationService, OrganizationService>();
        
        return services;
    }
}
