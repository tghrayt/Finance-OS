using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.Identity.Infrastructure;
using FinanceOS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Identity.Api");

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services
    .AddHealthChecks()
    .AddCheck<IdentityDatabaseHealthCheck>(
        "identity-db",
        tags: ["ready"]);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Identity:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Identity Service",
    phase = "Identity",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();

internal sealed class IdentityDatabaseHealthCheck(IdentityDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Identity database is reachable.")
            : HealthCheckResult.Unhealthy("Identity database is not reachable.");
    }
}
