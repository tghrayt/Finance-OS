using FinanceOS.Budget.Api.Endpoints;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.CreateMonthlyBudget;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.GetMonthlyBudget;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;
using FinanceOS.Budget.Infrastructure;
using FinanceOS.Budget.Infrastructure.Persistence;
using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Budget.Api");

builder.Services.AddBudgetInfrastructure(builder.Configuration);
builder.Services.AddFinanceOSJwtSecurity(builder.Configuration, builder.Environment);
builder.Services.AddScoped<CreateMonthlyBudgetHandler>();
builder.Services.AddScoped<GetMonthlyBudgetHandler>();
builder.Services.AddScoped<SetBudgetAllocationHandler>();
builder.Services
    .AddHealthChecks()
    .AddCheck<BudgetDatabaseHealthCheck>("budget-db", tags: ["ready"]);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Budget:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Budget Service",
    phase = "Budget Core",
    status = "Running"
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapBudgetEndpoints(app.Configuration, app.Environment);
app.MapFinanceOSHealthChecks();

app.Run();

internal sealed class BudgetDatabaseHealthCheck(BudgetDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Budget database is reachable.")
            : HealthCheckResult.Unhealthy("Budget database is not reachable.");
    }
}
