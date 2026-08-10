using FinanceOS.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args)
    .AddFinanceOSFoundation("FinanceOS.Gateway");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "FinanceOS API Gateway",
    phase = "Foundation",
    status = "Running"
}));

app.MapFinanceOSHealthChecks();
app.MapReverseProxy();

app.Run();
