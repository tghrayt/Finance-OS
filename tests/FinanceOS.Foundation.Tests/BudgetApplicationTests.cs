using FinanceOS.Budget.Application.Abstractions;
using FinanceOS.Budget.Application.Common;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.CreateMonthlyBudget;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;
using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Foundation.Tests;

public sealed class BudgetApplicationTests
{
    [Fact]
    public async Task CreateMonthlyBudgetPersistsBudget()
    {
        var budgets = new InMemoryMonthlyBudgetRepository();
        var handler = new CreateMonthlyBudgetHandler(budgets, new InMemoryBudgetUnitOfWork());

        var result = await handler.HandleAsync(new CreateMonthlyBudgetCommand(Guid.NewGuid(), 2026, 8, 1500, "EUR"), CancellationToken.None);

        Assert.Equal(1500, result.TotalBudget);
        Assert.Equal(8, result.Month);
    }

    [Fact]
    public async Task CreateMonthlyBudgetRejectsDuplicatePeriod()
    {
        var householdId = Guid.NewGuid();
        var budgets = new InMemoryMonthlyBudgetRepository();
        var handler = new CreateMonthlyBudgetHandler(budgets, new InMemoryBudgetUnitOfWork());
        await handler.HandleAsync(new CreateMonthlyBudgetCommand(householdId, 2026, 8, 1500, "EUR"), CancellationToken.None);

        await Assert.ThrowsAsync<BudgetConflictException>(() =>
            handler.HandleAsync(new CreateMonthlyBudgetCommand(householdId, 2026, 8, 2000, "EUR"), CancellationToken.None));
    }

    [Fact]
    public async Task SetBudgetAllocationAddsCategoryAllocation()
    {
        var budgets = new InMemoryMonthlyBudgetRepository();
        var budget = MonthlyBudget.Create(HouseholdId.New(), 2026, 8, 1500, "EUR", DateTimeOffset.UtcNow);
        await budgets.AddAsync(budget, CancellationToken.None);
        var handler = new SetBudgetAllocationHandler(budgets, new InMemoryBudgetUnitOfWork());

        var result = await handler.HandleAsync(new SetBudgetAllocationCommand(budget.Id.Value, Guid.NewGuid(), 300), CancellationToken.None);

        Assert.Single(result.Allocations);
        Assert.Equal(300, result.Allocations.Single().PlannedAmount);
    }

    private sealed class InMemoryMonthlyBudgetRepository : IMonthlyBudgetRepository
    {
        private readonly List<MonthlyBudget> _budgets = [];

        public Task AddAsync(MonthlyBudget budget, CancellationToken cancellationToken)
        {
            _budgets.Add(budget);
            return Task.CompletedTask;
        }

        public Task<MonthlyBudget?> GetByIdAsync(MonthlyBudgetId id, CancellationToken cancellationToken) =>
            Task.FromResult(_budgets.FirstOrDefault(budget => budget.Id == id));

        public Task<MonthlyBudget?> GetByPeriodAsync(HouseholdId householdId, int year, int month, CancellationToken cancellationToken) =>
            Task.FromResult(_budgets.FirstOrDefault(budget => budget.HouseholdId == householdId && budget.Year == year && budget.Month == month));
    }

    private sealed class InMemoryBudgetUnitOfWork : IBudgetUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
