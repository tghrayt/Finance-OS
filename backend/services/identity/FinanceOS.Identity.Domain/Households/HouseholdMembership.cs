using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Domain.Households;

public sealed class HouseholdMembership
{
    private HouseholdMembership()
    {
    }

    internal HouseholdMembership(UserId userId, HouseholdRole role, DateTimeOffset joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public UserId UserId { get; }

    public HouseholdRole Role { get; private set; }

    public DateTimeOffset JoinedAt { get; }

    internal void ChangeRole(HouseholdRole role)
    {
        Role = role;
    }
}
