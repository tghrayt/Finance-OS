using FinanceOS.Budget.Domain.Budgets;

namespace FinanceOS.Budget.Application.Features.MonthlyBudgets;

public sealed record BudgetAllocationResult(
    Guid AllocationId,
    Guid CategoryId,
    decimal PlannedAmount,
    decimal ActualAmount,
    string Currency,
    decimal ConsumptionRatio)
{
    public static BudgetAllocationResult FromAllocation(BudgetAllocation allocation) =>
        new(
            allocation.Id.Value,
            allocation.CategoryId,
            allocation.PlannedAmount,
            allocation.ActualAmount,
            allocation.Currency,
            allocation.ConsumptionRatio);
}
