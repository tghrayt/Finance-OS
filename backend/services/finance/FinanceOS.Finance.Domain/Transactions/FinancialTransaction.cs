using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Domain.Transactions;

public sealed class FinancialTransaction
{
    private FinancialTransaction()
    {
        Currency = "EUR";
        Merchant = string.Empty;
        Description = string.Empty;
    }

    private FinancialTransaction(
        TransactionId id,
        HouseholdId householdId,
        AccountId accountId,
        AccountId? destinationAccountId,
        TransactionType type,
        Money amount,
        CategoryId? categoryId,
        string? merchant,
        string? description,
        DateOnly transactionDate,
        DateTimeOffset createdAt)
    {
        Id = id;
        HouseholdId = householdId;
        AccountId = accountId;
        DestinationAccountId = destinationAccountId;
        Type = type;
        Amount = amount.Amount;
        Currency = amount.Currency.Value;
        CategoryId = categoryId;
        Merchant = merchant?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        TransactionDate = transactionDate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TransactionId Id { get; }
    public HouseholdId HouseholdId { get; }
    public AccountId AccountId { get; }
    public AccountId? DestinationAccountId { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public CategoryId? CategoryId { get; }
    public string Merchant { get; }
    public string Description { get; }
    public DateOnly TransactionDate { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; }

    public static FinancialTransaction Create(
        HouseholdId householdId,
        AccountId accountId,
        TransactionType type,
        decimal amount,
        string currency,
        CategoryId? categoryId,
        string? merchant,
        string? description,
        DateOnly transactionDate,
        AccountId? destinationAccountId = null,
        DateTimeOffset? createdAt = null)
    {
        if (householdId.Value == Guid.Empty || accountId.Value == Guid.Empty)
        {
            throw new InvalidTransactionException("Household and account are required.");
        }

        if (type == TransactionType.Transfer)
        {
            if (destinationAccountId is null || destinationAccountId.Value.Value == Guid.Empty)
            {
                throw new InvalidTransactionException("Destination account is required for transfers.");
            }

            if (destinationAccountId.Value == accountId)
            {
                throw new InvalidTransactionException("Transfer source and destination accounts must be different.");
            }
        }
        else if (destinationAccountId is not null)
        {
            throw new InvalidTransactionException("Destination account is only allowed for transfers.");
        }

        return new FinancialTransaction(
            TransactionId.New(),
            householdId,
            accountId,
            destinationAccountId,
            type,
            Money.Positive(amount, currency),
            categoryId,
            merchant,
            description,
            transactionDate,
            createdAt ?? SystemClock.UtcNow);
    }

    public TransactionImpact SourceImpact => Type is TransactionType.Income or TransactionType.Refund
        ? TransactionImpact.Credit
        : TransactionImpact.Debit;
}

public sealed class InvalidTransactionException(string message) : DomainException(message);
