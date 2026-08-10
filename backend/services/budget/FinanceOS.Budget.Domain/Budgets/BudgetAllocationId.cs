namespace FinanceOS.Budget.Domain.Budgets;

public readonly record struct BudgetAllocationId(Guid Value)
{
    public static BudgetAllocationId New() => new(Guid.NewGuid());
}
