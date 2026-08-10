namespace FinanceOS.Budget.Domain.Budgets;

public readonly record struct MonthlyBudgetId(Guid Value)
{
    public static MonthlyBudgetId New() => new(Guid.NewGuid());
}
