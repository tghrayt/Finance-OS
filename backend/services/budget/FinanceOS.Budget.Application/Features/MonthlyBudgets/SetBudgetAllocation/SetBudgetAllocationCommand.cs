namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;

public sealed record SetBudgetAllocationCommand(Guid HouseholdId, Guid BudgetId, Guid CategoryId, decimal PlannedAmount);
