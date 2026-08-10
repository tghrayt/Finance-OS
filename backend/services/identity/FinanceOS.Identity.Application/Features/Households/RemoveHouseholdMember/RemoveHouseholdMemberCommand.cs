namespace FinanceOS.Identity.Application.Features.Households.RemoveHouseholdMember;

public sealed record RemoveHouseholdMemberCommand(
    Guid HouseholdId,
    Guid ActorUserId,
    Guid UserId);
