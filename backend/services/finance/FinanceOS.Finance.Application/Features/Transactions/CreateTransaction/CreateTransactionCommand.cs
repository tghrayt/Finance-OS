namespace FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid HouseholdId,
    Guid AccountId,
    Guid? DestinationAccountId,
    string Type,
    decimal Amount,
    string Currency,
    Guid? CategoryId,
    string? Merchant,
    string? Description,
    DateOnly TransactionDate,
    Guid CorrelationId);
