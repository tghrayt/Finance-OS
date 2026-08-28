using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Domain.Accounts;

public sealed class Account
{
    private Account()
    {
        Name = string.Empty;
        Currency = "EUR";
        InstitutionName = string.Empty;
    }

    private Account(
        AccountId id,
        HouseholdId householdId,
        string name,
        AccountType type,
        Money initialBalance,
        string? institutionName,
        DateTimeOffset createdAt)
    {
        Id = id;
        HouseholdId = householdId;
        Name = name;
        Type = type;
        Currency = initialBalance.Currency.Value;
        InitialBalance = initialBalance.Amount;
        CurrentBalance = initialBalance.Amount;
        InstitutionName = institutionName?.Trim() ?? string.Empty;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public AccountId Id { get; }
    public HouseholdId HouseholdId { get; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public string Currency { get; private set; }
    public decimal InitialBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public string InstitutionName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Account Create(
        HouseholdId householdId,
        string name,
        AccountType type,
        decimal initialBalance,
        string currency,
        string? institutionName = null,
        DateTimeOffset? createdAt = null)
    {
        if (householdId.Value == Guid.Empty)
        {
            throw new InvalidAccountException("Household is required.");
        }

        return new Account(
            AccountId.New(),
            householdId,
            RequiredName(name),
            type,
            Money.Create(initialBalance, currency),
            institutionName,
            createdAt ?? SystemClock.UtcNow);
    }

    public void Apply(TransactionImpact impact, Money amount, DateTimeOffset? updatedAt = null)
    {
        if (Currency != amount.Currency.Value)
        {
            throw new InvalidAccountException("Transaction currency must match account currency.");
        }

        CurrentBalance = impact == TransactionImpact.Credit
            ? CurrentBalance + amount.Amount
            : CurrentBalance - amount.Amount;
        UpdatedAt = updatedAt ?? SystemClock.UtcNow;
    }

    public void Archive(DateTimeOffset? updatedAt = null)
    {
        IsActive = false;
        UpdatedAt = updatedAt ?? SystemClock.UtcNow;
    }

    public void UpdateDetails(string name, AccountType type, string? institutionName, DateTimeOffset? updatedAt = null)
    {
        Name = RequiredName(name);
        Type = type;
        InstitutionName = institutionName?.Trim() ?? string.Empty;
        UpdatedAt = updatedAt ?? SystemClock.UtcNow;
    }

    private static string RequiredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidAccountException("Account name is required.");
        }

        return name.Trim();
    }
}

public enum TransactionImpact
{
    Debit = 1,
    Credit = 2
}

public sealed class InvalidAccountException(string message) : DomainException(message);
