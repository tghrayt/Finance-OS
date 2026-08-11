using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceOS.Budget.Infrastructure.Messaging;

internal sealed class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IPublishEndpoint publishEndpoint,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishBatchAsync(stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Persistence.BudgetDbContext>();
        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(20)
            .ToArrayAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var type = Type.GetType(message.Type, throwOnError: true)!;
                var payload = JsonSerializer.Deserialize(message.Content, type)
                    ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");

                await publishEndpoint.Publish(payload, type, cancellationToken);
                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to publish budget outbox message {MessageId}", message.Id);
                message.MarkFailed(exception.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
