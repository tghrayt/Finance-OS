using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Domain.Budgets;

public sealed class BudgetAllocation
{
    private BudgetAllocation()
    {
    }

    internal BudgetAllocation(Guid categoryId, Money plannedAmount)
    {
        Id = BudgetAllocationId.New();
        CategoryId = categoryId;
        PlannedAmount = plannedAmount.Amount;
        ActualAmount = 0;
        Currency = plannedAmount.Currency.Value;
    }

    public BudgetAllocationId Id { get; }

    public Guid CategoryId { get; }

    public decimal PlannedAmount { get; private set; }

    public decimal ActualAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public decimal ConsumptionRatio => PlannedAmount == 0 ? 0 : ActualAmount / PlannedAmount;

    public void ReplacePlannedAmount(Money plannedAmount)
    {
        EnsureSameCurrency(plannedAmount);
        PlannedAmount = plannedAmount.Amount;
    }

    public void AddActualAmount(Money amount)
    {
        EnsureSameCurrency(amount);
        ActualAmount = decimal.Round(ActualAmount + amount.Amount, 2);
    }

    private void EnsureSameCurrency(Money money)
    {
        if (Currency != money.Currency.Value)
        {
            throw new InvalidBudgetException("Allocation currency must match the budget currency.");
        }
    }
}
