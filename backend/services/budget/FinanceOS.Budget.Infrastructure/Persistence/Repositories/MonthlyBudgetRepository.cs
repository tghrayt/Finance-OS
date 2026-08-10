using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Budget.Infrastructure.Persistence.Repositories;

internal sealed class MonthlyBudgetRepository(BudgetDbContext dbContext) : IMonthlyBudgetRepository
{
    public async Task AddAsync(MonthlyBudget budget, CancellationToken cancellationToken) =>
        await dbContext.MonthlyBudgets.AddAsync(budget, cancellationToken);

    public async Task<MonthlyBudget?> GetByIdAsync(MonthlyBudgetId id, CancellationToken cancellationToken) =>
        await dbContext.MonthlyBudgets.FirstOrDefaultAsync(budget => budget.Id == id, cancellationToken);

    public async Task<MonthlyBudget?> GetByPeriodAsync(HouseholdId householdId, int year, int month, CancellationToken cancellationToken) =>
        await dbContext.MonthlyBudgets.FirstOrDefaultAsync(
            budget => budget.HouseholdId == householdId && budget.Year == year && budget.Month == month,
            cancellationToken);
}
