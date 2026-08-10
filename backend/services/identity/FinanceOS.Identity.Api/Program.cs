using FinanceOS.BuildingBlocks.Observability;
using FinanceOS.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Identity.Api");

builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Identity Service",
    phase = "Identity",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
