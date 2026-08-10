using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Application.Abstractions;

public interface IMonthlyBudgetRepository
{
    Task AddAsync(MonthlyBudget budget, CancellationToken cancellationToken);

    Task<MonthlyBudget?> GetByIdAsync(MonthlyBudgetId id, CancellationToken cancellationToken);

    Task<MonthlyBudget?> GetByPeriodAsync(HouseholdId householdId, int year, int month, CancellationToken cancellationToken);
}
