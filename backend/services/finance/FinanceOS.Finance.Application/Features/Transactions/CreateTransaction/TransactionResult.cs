using FinanceOS.Finance.Domain.Transactions;

namespace FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;

public sealed record TransactionResult(
    Guid TransactionId,
    Guid HouseholdId,
    Guid AccountId,
    Guid? DestinationAccountId,
    string Type,
    decimal Amount,
    string Currency,
    Guid? CategoryId,
    DateOnly TransactionDate)
{
    public static TransactionResult FromTransaction(FinancialTransaction transaction) =>
        new(
            transaction.Id.Value,
            transaction.HouseholdId.Value,
            transaction.AccountId.Value,
            transaction.DestinationAccountId?.Value,
            transaction.Type.ToString(),
            transaction.Amount,
            transaction.Currency,
            transaction.CategoryId?.Value,
            transaction.TransactionDate);
}
