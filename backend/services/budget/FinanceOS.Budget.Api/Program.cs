using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Budget.Api");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Budget Service",
    phase = "Foundation",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
