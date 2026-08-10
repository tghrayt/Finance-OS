namespace FinanceOS.Budget.Domain.Common;

public readonly record struct HouseholdId(Guid Value)
{
    public static HouseholdId New() => new(Guid.NewGuid());
}
