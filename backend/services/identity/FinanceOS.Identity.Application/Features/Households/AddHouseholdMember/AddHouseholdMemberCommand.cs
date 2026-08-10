namespace FinanceOS.Identity.Application.Features.Households.AddHouseholdMember;

public sealed record AddHouseholdMemberCommand(
    Guid HouseholdId,
    Guid ActorUserId,
    Guid UserId,
    string Role);
