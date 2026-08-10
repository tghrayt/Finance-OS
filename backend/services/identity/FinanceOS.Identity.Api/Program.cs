using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Identity.Api");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Identity Service",
    phase = "Identity",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
