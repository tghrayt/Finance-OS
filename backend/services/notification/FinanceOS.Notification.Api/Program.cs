using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Notification.Api");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS Notification Service",
    phase = "Foundation",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();

app.Run();
