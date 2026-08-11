using FinanceOS.Contracts.Budget;
using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;
using FinanceOS.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Notification.Infrastructure.Messaging;

public sealed class BudgetExceededConsumer(NotificationDbContext dbContext) : IConsumer<BudgetExceededV1>
{
    private const string ConsumerName = nameof(BudgetExceededConsumer);

    public async Task Consume(ConsumeContext<BudgetExceededV1> context)
    {
        var message = context.Message;
        if (await AlreadyProcessedAsync(message.EventId, context.CancellationToken))
        {
            return;
        }

        dbContext.InAppNotifications.Add(InAppNotification.Create(
            new HouseholdId(message.HouseholdId),
            "BudgetExceeded",
            "Budget depasse",
            $"Une categorie a depasse son budget avec {message.ActualAmount:0.##} {message.Currency} consommes pour {message.PlannedAmount:0.##} {message.Currency} prevus.",
            DateTimeOffset.UtcNow));

        await MarkProcessedAsync(message.EventId, context.CancellationToken);
    }

    private async Task<bool> AlreadyProcessedAsync(Guid eventId, CancellationToken cancellationToken) =>
        await dbContext.InboxMessages.AnyAsync(item => item.MessageId == eventId && item.ConsumerName == ConsumerName, cancellationToken);

    private async Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        dbContext.InboxMessages.Add(new InboxMessage(eventId, ConsumerName, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
