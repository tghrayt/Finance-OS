using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Application.Common;
using FinanceOS.Budget.Application.Features.MonthlyBudgets;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.GetMonthlyBudget;

public sealed class GetMonthlyBudgetHandler(IMonthlyBudgetRepository budgets)
{
    public async Task<MonthlyBudgetResult> HandleAsync(Guid householdId, int year, int month, CancellationToken cancellationToken)
    {
        var budget = await budgets.GetByPeriodAsync(new HouseholdId(householdId), year, month, cancellationToken)
            ?? throw new BudgetNotFoundException("Budget was not found.");

        return MonthlyBudgetResult.FromBudget(budget);
    }
}
