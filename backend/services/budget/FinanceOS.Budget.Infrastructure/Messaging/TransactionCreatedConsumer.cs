using FinanceOS.Budget.Domain.Common;
using FinanceOS.Budget.Infrastructure.Persistence;
using FinanceOS.Contracts.Finance;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceOS.Budget.Infrastructure.Messaging;

public sealed class TransactionCreatedConsumer(BudgetDbContext dbContext, ILogger<TransactionCreatedConsumer> logger) : IConsumer<TransactionCreatedV1>
{
    private const string ConsumerName = nameof(TransactionCreatedConsumer);

    public async Task Consume(ConsumeContext<TransactionCreatedV1> context)
    {
        var message = context.Message;
        var alreadyProcessed = await dbContext.Set<InboxMessage>()
            .AnyAsync(item => item.MessageId == message.EventId && item.ConsumerName == ConsumerName, context.CancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        if (!IsBudgetExpense(message))
        {
            await MarkProcessedAsync(message.EventId, context.CancellationToken);
            return;
        }

        var categoryId = message.CategoryId.GetValueOrDefault();
        var budget = await dbContext.MonthlyBudgets.FirstOrDefaultAsync(
            item =>
                item.HouseholdId == new HouseholdId(message.HouseholdId) &&
                item.Year == message.TransactionDate.Year &&
                item.Month == message.TransactionDate.Month,
            context.CancellationToken);

        if (budget is null)
        {
            logger.LogInformation(
                "No monthly budget found for transaction {TransactionId} and household {HouseholdId}",
                message.TransactionId,
                message.HouseholdId);
            await MarkProcessedAsync(message.EventId, context.CancellationToken);
            return;
        }

        budget.AddExpense(categoryId, message.Amount, message.Currency);
        await MarkProcessedAsync(message.EventId, context.CancellationToken);
    }

    private static bool IsBudgetExpense(TransactionCreatedV1 message) =>
        message.CategoryId is not null && string.Equals(message.Type, "Expense", StringComparison.OrdinalIgnoreCase);

    private async Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        dbContext.Set<InboxMessage>().Add(new InboxMessage(eventId, ConsumerName, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
