namespace FinanceOS.Contracts.Finance;

public sealed record TransactionCreatedV1(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid CorrelationId,
    Guid TransactionId,
    Guid HouseholdId,
    Guid AccountId,
    Guid? DestinationAccountId,
    string Type,
    decimal Amount,
    string Currency,
    Guid? CategoryId,
    DateOnly TransactionDate);
