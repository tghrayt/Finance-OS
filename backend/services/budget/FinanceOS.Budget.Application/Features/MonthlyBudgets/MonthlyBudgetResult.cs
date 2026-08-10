using FinanceOS.Budget.Domain.Budgets;

namespace FinanceOS.Budget.Application.Features.MonthlyBudgets;

public sealed record MonthlyBudgetResult(
    Guid BudgetId,
    Guid HouseholdId,
    int Year,
    int Month,
    decimal TotalBudget,
    decimal ActualAmount,
    string Currency,
    decimal ConsumptionRatio,
    IReadOnlyCollection<BudgetAllocationResult> Allocations)
{
    public static MonthlyBudgetResult FromBudget(MonthlyBudget budget) =>
        new(
            budget.Id.Value,
            budget.HouseholdId.Value,
            budget.Year,
            budget.Month,
            budget.TotalBudget,
            budget.ActualAmount,
            budget.Currency,
            budget.ConsumptionRatio,
            budget.Allocations.Select(BudgetAllocationResult.FromAllocation).ToArray());
}
