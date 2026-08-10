using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FinanceOS.Identity.Api.Security;

internal static class IdentitySecurityExtensions
{
    public static IServiceCollection AddIdentityApiSecurity(
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
            options.AddPolicy(IdentityPolicies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(IdentityPolicies.CanManageHousehold, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }

    public static bool ShouldRequireIdentityAuthorization(
        this IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return configuration.GetValue<bool?>("Authentication:Jwt:RequireAuthorization")
            ?? !environment.IsDevelopment();
    }
}
