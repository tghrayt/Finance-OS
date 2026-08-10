using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.Finance.Api.Endpoints;
using FinanceOS.Finance.Application.Features.Accounts.CreateAccount;
using FinanceOS.Finance.Application.Features.Accounts.GetAccounts;
using FinanceOS.Finance.Application.Features.Categories.CreateCategory;
using FinanceOS.Finance.Application.Features.Categories.GetCategories;
using FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;
using FinanceOS.Finance.Application.Features.Transactions.GetTransactions;
using FinanceOS.Finance.Infrastructure;
using FinanceOS.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Finance.Api");

builder.Services.AddFinanceInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateAccountHandler>();
builder.Services.AddScoped<GetAccountsHandler>();
builder.Services.AddScoped<CreateCategoryHandler>();
builder.Services.AddScoped<GetCategoriesHandler>();
builder.Services.AddScoped<CreateTransactionHandler>();
builder.Services.AddScoped<GetTransactionsHandler>();
builder.Services
    .AddHealthChecks()
    .AddCheck<FinanceDatabaseHealthCheck>("finance-db", tags: ["ready"]);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Finance:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Finance Service",
    phase = "Finance Core",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();
app.MapFinanceEndpoints();

app.Run();

internal sealed class FinanceDatabaseHealthCheck(FinanceDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("Finance database is reachable.")
            : HealthCheckResult.Unhealthy("Finance database is not reachable.");
    }
}
