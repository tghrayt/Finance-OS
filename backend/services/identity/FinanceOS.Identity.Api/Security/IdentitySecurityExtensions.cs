using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

                options.MapInboundClaims = false;
                options.Audience = jwtAudience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = BuildValidAudiences(jwtAudience),
                    ValidIssuers = BuildValidIssuers(jwtAuthority),
                    NameClaimType = "name",
                    RoleClaimType = "roles"
                };
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

    private static IReadOnlyCollection<string> BuildValidAudiences(string? configuredAudience)
    {
        if (string.IsNullOrWhiteSpace(configuredAudience))
        {
            return [];
        }

        var audiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            configuredAudience
        };

        const string appIdUriPrefix = "api://";
        if (configuredAudience.StartsWith(appIdUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            audiences.Add(configuredAudience[appIdUriPrefix.Length..]);
        }
        else
        {
            audiences.Add($"{appIdUriPrefix}{configuredAudience}");
        }

        return audiences;
    }

    private static IReadOnlyCollection<string> BuildValidIssuers(string? configuredAuthority)
    {
        if (string.IsNullOrWhiteSpace(configuredAuthority))
        {
            return [];
        }

        return
        [
            configuredAuthority.TrimEnd('/'),
            $"{configuredAuthority.TrimEnd('/')}/"
        ];
    }

    public static bool ShouldRequireIdentityAuthorization(
        this IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return configuration.GetValue<bool?>("Authentication:Jwt:RequireAuthorization")
            ?? !environment.IsDevelopment();
    }
}
