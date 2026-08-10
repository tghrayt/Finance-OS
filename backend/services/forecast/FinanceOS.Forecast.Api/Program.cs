using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Forecast.Api");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Forecast Service",
    phase = "Foundation",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
