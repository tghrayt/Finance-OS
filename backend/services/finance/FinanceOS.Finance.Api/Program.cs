using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Finance.Api");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Finance Service",
    phase = "Foundation",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
