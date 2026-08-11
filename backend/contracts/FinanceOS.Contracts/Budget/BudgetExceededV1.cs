namespace FinanceOS.Contracts.Budget;

public sealed record BudgetExceededV1(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid HouseholdId,
    Guid BudgetId,
    Guid CategoryId,
    decimal PlannedAmount,
    decimal ActualAmount,
    string Currency);
