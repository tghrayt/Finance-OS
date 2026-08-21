using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;

namespace FinanceOS.BuildingBlocks.Security;

public static class FinanceOSJwtSecurityExtensions
{
    public const string AuthenticatedUserPolicy = "AuthenticatedUser";

    public static IServiceCollection AddFinanceOSJwtSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwtAuthority = configuration["Authentication:Jwt:Authority"];
        var jwtAudience = configuration["Authentication:Jwt:Audience"];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                if (!string.IsNullOrWhiteSpace(jwtAuthority))
                {
                    options.Authority = jwtAuthority;
                }

                options.Audience = jwtAudience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthenticatedUserPolicy, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }

    public static RouteGroupBuilder RequireFinanceOSAuthorization(
        this RouteGroupBuilder group,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return configuration.ShouldRequireFinanceOSAuthorization(environment)
            ? group.RequireAuthorization(AuthenticatedUserPolicy)
            : group;
    }

    public static bool ShouldRequireFinanceOSAuthorization(
        this IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return configuration.GetValue<bool?>("Authentication:Jwt:RequireAuthorization")
            ?? !environment.IsDevelopment();
    }
}
