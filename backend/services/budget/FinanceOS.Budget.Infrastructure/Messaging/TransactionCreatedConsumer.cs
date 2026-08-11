using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;
using FinanceOS.Budget.Infrastructure.Persistence;
using FinanceOS.Contracts.Budget;
using FinanceOS.Contracts.Finance;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceOS.Budget.Infrastructure.Messaging;

public sealed class TransactionCreatedConsumer(
    BudgetDbContext dbContext,
    IOutboxWriter outbox,
    ILogger<TransactionCreatedConsumer> logger) : IConsumer<TransactionCreatedV1>
{
    private const string ConsumerName = nameof(TransactionCreatedConsumer);
    private static readonly decimal[] Thresholds = [0.50m, 0.75m, 0.90m, 1.00m];

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

        var allocation = budget.Allocations.FirstOrDefault(item => item.CategoryId == categoryId);
        if (allocation is null)
        {
            await MarkProcessedAsync(message.EventId, context.CancellationToken);
            return;
        }

        var previousRatio = allocation.ConsumptionRatio;
        budget.AddExpense(categoryId, message.Amount, message.Currency);
        AddThresholdEvents(message, budget.Id.Value, allocation, previousRatio);
        await MarkProcessedAsync(message.EventId, context.CancellationToken);
    }

    private static bool IsBudgetExpense(TransactionCreatedV1 message) =>
        message.CategoryId is not null && string.Equals(message.Type, "Expense", StringComparison.OrdinalIgnoreCase);

    private async Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        dbContext.Set<InboxMessage>().Add(new InboxMessage(eventId, ConsumerName, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddThresholdEvents(TransactionCreatedV1 message, Guid budgetId, BudgetAllocation allocation, decimal previousRatio)
    {
        var currentRatio = allocation.ConsumptionRatio;
        foreach (var threshold in Thresholds.Where(threshold => previousRatio < threshold && currentRatio >= threshold))
        {
            outbox.Add(new BudgetThresholdReachedV1(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                message.CorrelationId,
                message.HouseholdId,
                budgetId,
                allocation.CategoryId,
                threshold,
                allocation.PlannedAmount,
                allocation.ActualAmount,
                allocation.Currency));
        }

        if (previousRatio <= 1.00m && currentRatio > 1.00m)
        {
            outbox.Add(new BudgetExceededV1(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                message.CorrelationId,
                message.HouseholdId,
                budgetId,
                allocation.CategoryId,
                allocation.PlannedAmount,
                allocation.ActualAmount,
                allocation.Currency));
        }
    }
}
