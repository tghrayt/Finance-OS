namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;

public sealed record SetBudgetAllocationCommand(Guid BudgetId, Guid CategoryId, decimal PlannedAmount);
