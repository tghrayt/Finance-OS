using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.Notification.Api.Endpoints;
using FinanceOS.Notification.Application.Features.InApp.GetNotifications;
using FinanceOS.Notification.Application.Features.InApp.MarkNotificationRead;
using FinanceOS.Notification.Infrastructure;
using FinanceOS.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Notification.Api");

builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddScoped<GetNotificationsHandler>();
builder.Services.AddScoped<MarkNotificationReadHandler>();
builder.Services
    .AddHealthChecks()
    .AddCheck<NotificationDatabaseHealthCheck>("notification-db", tags: ["ready"]);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Notification:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Notification Service",
    phase = "Notification Core",
    status = "Running"
}));

app.MapNotificationEndpoints();
app.MapFinanceOSHealthChecks();

app.Run();

internal sealed class NotificationDatabaseHealthCheck(NotificationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Notification database is reachable.")
            : HealthCheckResult.Unhealthy("Notification database is not reachable.");
    }
}
