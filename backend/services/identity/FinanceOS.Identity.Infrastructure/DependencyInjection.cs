using FinanceOS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceOS.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDatabase")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=financeos;Username=financeos;Password=financeos";

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
