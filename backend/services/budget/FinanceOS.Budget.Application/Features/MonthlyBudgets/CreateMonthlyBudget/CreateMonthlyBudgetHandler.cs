using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Application.Common;
using FinanceOS.Budget.Application.Features.MonthlyBudgets;
using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Application.Features.MonthlyBudgets.CreateMonthlyBudget;

public sealed class CreateMonthlyBudgetHandler(IMonthlyBudgetRepository budgets, IBudgetUnitOfWork unitOfWork)
{
    public async Task<MonthlyBudgetResult> HandleAsync(CreateMonthlyBudgetCommand command, CancellationToken cancellationToken)
    {
        var householdId = new HouseholdId(command.HouseholdId);
        var existing = await budgets.GetByPeriodAsync(householdId, command.Year, command.Month, cancellationToken);
        if (existing is not null)
        {
            throw new BudgetConflictException("A budget already exists for this month.");
        }

        var budget = MonthlyBudget.Create(householdId, command.Year, command.Month, command.TotalBudget, command.Currency, DateTimeOffset.UtcNow);
        await budgets.AddAsync(budget, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MonthlyBudgetResult.FromBudget(budget);
    }
}
