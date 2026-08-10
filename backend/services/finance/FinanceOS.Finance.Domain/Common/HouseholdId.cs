namespace FinanceOS.Finance.Domain.Common;

public readonly record struct HouseholdId(Guid Value)
{
    public static HouseholdId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
