using FinanceOS.Identity.Domain.Households;

namespace FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;

public sealed record HouseholdDetailsResult(
    Guid HouseholdId,
    string Name,
    string Currency,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<HouseholdMemberResult> Members)
{
    public static HouseholdDetailsResult FromHousehold(Household household)
    {
        return new HouseholdDetailsResult(
            household.Id.Value,
            household.Name,
            household.Currency,
            household.OwnerId.Value,
            household.CreatedAt,
            household.Memberships
                .Select(membership => new HouseholdMemberResult(
                    membership.UserId.Value,
                    membership.Role.ToString(),
                    membership.JoinedAt))
                .ToArray());
    }
}

public sealed record HouseholdMemberResult(
    Guid UserId,
    string Role,
    DateTimeOffset JoinedAt);
