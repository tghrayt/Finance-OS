namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.CreateMonthlyBudget;

public sealed record CreateMonthlyBudgetCommand(Guid HouseholdId, int Year, int Month, decimal TotalBudget, string Currency);
