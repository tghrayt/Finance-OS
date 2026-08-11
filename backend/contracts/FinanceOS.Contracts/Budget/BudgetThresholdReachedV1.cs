namespace FinanceOS.Contracts.Budget;

public sealed record BudgetThresholdReachedV1(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid HouseholdId,
    Guid BudgetId,
    Guid CategoryId,
    decimal Threshold,
    decimal PlannedAmount,
    decimal ActualAmount,
    string Currency);
