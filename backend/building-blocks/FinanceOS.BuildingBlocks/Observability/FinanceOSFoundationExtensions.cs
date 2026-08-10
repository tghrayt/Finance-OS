using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace FinanceOS.BuildingBlocks.Observability;

public static class FinanceOSFoundationExtensions
{
    public static WebApplicationBuilder AddFinanceOSFoundation(this WebApplicationBuilder builder, string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console();
        });

        builder.Services.AddHealthChecks();
        builder.Services.AddFinanceOSOpenTelemetry(builder.Configuration, serviceName);

        return builder;
    }

    public static IEndpointRouteBuilder MapFinanceOSHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/health/live");
        endpoints.MapHealthChecks("/health/ready");

        return endpoints;
    }

    private static IServiceCollection AddFinanceOSOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddRuntimeInstrumentation();

                if (ShouldUseOtlp(configuration))
                {
                    metrics.AddOtlpExporter();
                }
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();

                if (ShouldUseOtlp(configuration))
                {
                    tracing.AddOtlpExporter();
                }
            });

        return services;
    }

    private static bool ShouldUseOtlp(IConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    }
}
