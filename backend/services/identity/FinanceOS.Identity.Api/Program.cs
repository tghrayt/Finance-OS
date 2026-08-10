using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.Identity.Api.Endpoints;
using FinanceOS.Identity.Api.Security;
using FinanceOS.Identity.Application.Features.Households.AddHouseholdMember;
using FinanceOS.Identity.Application.Features.Households.ChangeHouseholdMemberRole;
using FinanceOS.Identity.Application.Features.Households.CreateHousehold;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Households.GetHousehold;
using FinanceOS.Identity.Application.Features.Households.RemoveHouseholdMember;
using FinanceOS.Identity.Application.Features.Users.CreateUser;
using FinanceOS.Identity.Application.Features.Users.GetUser;
using FinanceOS.Identity.Application.Features.Users.UpdateUserProfile;
using FinanceOS.Identity.Infrastructure;
using FinanceOS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Identity.Api");

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddIdentityApiSecurity(builder.Configuration, builder.Environment);
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetUserHandler>();
builder.Services.AddScoped<UpdateUserProfileHandler>();
builder.Services.AddScoped<CreateHouseholdHandler>();
builder.Services.AddScoped<GetHouseholdHandler>();
builder.Services.AddScoped<GetCurrentHouseholdHandler>();
builder.Services.AddScoped<AddHouseholdMemberHandler>();
builder.Services.AddScoped<ChangeHouseholdMemberRoleHandler>();
builder.Services.AddScoped<RemoveHouseholdMemberHandler>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapFinanceOSHealthChecks();
app.MapIdentityEndpoints(app.Configuration, app.Environment);

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
