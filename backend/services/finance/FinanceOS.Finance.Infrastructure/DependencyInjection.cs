using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Infrastructure.Messaging;
using FinanceOS.Finance.Infrastructure.Persistence;
using FinanceOS.Finance.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceOS.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FinanceDatabase")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=financeos;Username=financeos;Password=financeos";

        services.AddDbContext<FinanceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFinanceUnitOfWork, FinanceUnitOfWork>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<OutboxPublisher>();

        return services;
    }
}
