using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Foundation.Tests;

public sealed class BudgetDomainTests
{
    [Fact]
    public void MonthlyBudgetRejectsInvalidMonth()
    {
        Assert.Throws<InvalidBudgetException>(() =>
            MonthlyBudget.Create(HouseholdId.New(), 2026, 13, 1000, "EUR", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MonthlyBudgetTracksAllocationConsumption()
    {
        var categoryId = Guid.NewGuid();
        var budget = MonthlyBudget.Create(HouseholdId.New(), 2026, 8, 1000, "EUR", DateTimeOffset.UtcNow);

        budget.AddOrReplaceAllocation(categoryId, 300);
        budget.AddExpense(categoryId, 75, "EUR");

        var allocation = Assert.Single(budget.Allocations);
        Assert.Equal(75, allocation.ActualAmount);
        Assert.Equal(0.25m, allocation.ConsumptionRatio);
        Assert.Equal(75, budget.ActualAmount);
    }

    [Fact]
    public void AllocationConsumptionRatioCanCrossBudgetThresholds()
    {
        var categoryId = Guid.NewGuid();
        var budget = MonthlyBudget.Create(HouseholdId.New(), 2026, 8, 1000, "EUR", DateTimeOffset.UtcNow);
        budget.AddOrReplaceAllocation(categoryId, 100);
        var allocation = budget.Allocations.Single();

        var previousRatio = allocation.ConsumptionRatio;
        budget.AddExpense(categoryId, 91, "EUR");

        Assert.Equal(0, previousRatio);
        Assert.Equal(0.91m, allocation.ConsumptionRatio);
    }
}
