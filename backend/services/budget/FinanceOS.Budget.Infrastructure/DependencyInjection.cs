using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Infrastructure.Messaging;
using FinanceOS.Budget.Infrastructure.Persistence;
using FinanceOS.Budget.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceOS.Budget.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BudgetDatabase")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=financeos_budget;Username=financeos;Password=financeos";

        services.AddDbContext<BudgetDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IMonthlyBudgetRepository, MonthlyBudgetRepository>();
        services.AddScoped<IBudgetUnitOfWork, BudgetUnitOfWork>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<TransactionCreatedConsumer>();
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

        return services;
    }
}
