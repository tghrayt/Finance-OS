using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Application.Common;
using FinanceOS.Budget.Application.Features.MonthlyBudgets;
using FinanceOS.Budget.Domain.Budgets;

namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;

public sealed class SetBudgetAllocationHandler(IMonthlyBudgetRepository budgets, IBudgetUnitOfWork unitOfWork)
{
    public async Task<MonthlyBudgetResult> HandleAsync(SetBudgetAllocationCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgets.GetByIdAsync(new MonthlyBudgetId(command.BudgetId), cancellationToken)
            ?? throw new BudgetNotFoundException("Budget was not found.");

        budget.AddOrReplaceAllocation(command.CategoryId, command.PlannedAmount);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MonthlyBudgetResult.FromBudget(budget);
    }
}
