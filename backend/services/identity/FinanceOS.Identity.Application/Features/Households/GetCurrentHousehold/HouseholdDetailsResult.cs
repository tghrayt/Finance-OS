namespace FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;

public sealed record HouseholdDetailsResult(
    Guid HouseholdId,
    string Name,
    string Currency,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<HouseholdMemberResult> Members);

public sealed record HouseholdMemberResult(
    Guid UserId,
    string Role,
    DateTimeOffset JoinedAt);
