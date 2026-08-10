using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Domain.Budgets;

public sealed class MonthlyBudget
{
    private readonly List<BudgetAllocation> _allocations = [];

    private MonthlyBudget()
    {
    }

    private MonthlyBudget(MonthlyBudgetId id, HouseholdId householdId, int year, int month, Money totalBudget, DateTimeOffset createdAt)
    {
        Id = id;
        HouseholdId = householdId;
        Year = year;
        Month = month;
        TotalBudget = totalBudget.Amount;
        Currency = totalBudget.Currency.Value;
        CreatedAt = createdAt;
    }

    public MonthlyBudgetId Id { get; }

    public HouseholdId HouseholdId { get; }

    public int Year { get; }

    public int Month { get; }

    public decimal TotalBudget { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<BudgetAllocation> Allocations => _allocations.AsReadOnly();

    public decimal ActualAmount => _allocations.Sum(allocation => allocation.ActualAmount);

    public decimal ConsumptionRatio => TotalBudget == 0 ? 0 : ActualAmount / TotalBudget;

    public static MonthlyBudget Create(HouseholdId householdId, int year, int month, decimal totalBudget, string currency, DateTimeOffset createdAt)
    {
        if (householdId.Value == Guid.Empty)
        {
            throw new InvalidBudgetException("Household is required.");
        }

        if (year is < 2000 or > 2100)
        {
            throw new InvalidBudgetException("Budget year is invalid.");
        }

        if (month is < 1 or > 12)
        {
            throw new InvalidBudgetException("Budget month is invalid.");
        }

        return new MonthlyBudget(MonthlyBudgetId.New(), householdId, year, month, Money.Positive(totalBudget, currency), createdAt);
    }

    public void AddOrReplaceAllocation(Guid categoryId, decimal plannedAmount)
    {
        if (categoryId == Guid.Empty)
        {
            throw new InvalidBudgetException("Category is required.");
        }

        var money = Money.Positive(plannedAmount, Currency);
        var existing = _allocations.FirstOrDefault(allocation => allocation.CategoryId == categoryId);
        if (existing is null)
        {
            _allocations.Add(new BudgetAllocation(categoryId, money));
            return;
        }

        existing.ReplacePlannedAmount(money);
    }

    public void AddExpense(Guid categoryId, decimal amount, string currency)
    {
        var allocation = _allocations.FirstOrDefault(item => item.CategoryId == categoryId);
        allocation?.AddActualAmount(Money.Positive(amount, currency));
    }
}

public sealed class InvalidBudgetException(string message) : DomainException(message);
