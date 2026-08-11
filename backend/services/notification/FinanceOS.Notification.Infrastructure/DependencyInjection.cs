using FinanceOS.Notification.Application.Abstractions;
using FinanceOS.Notification.Infrastructure.Messaging;
using FinanceOS.Notification.Infrastructure.Persistence;
using FinanceOS.Notification.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceOS.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDatabase")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=financeos_notification;Username=financeos;Password=financeos";

        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IInAppNotificationRepository, InAppNotificationRepository>();
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<BudgetThresholdReachedConsumer>();
            bus.AddConsumer<BudgetExceededConsumer>();
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
