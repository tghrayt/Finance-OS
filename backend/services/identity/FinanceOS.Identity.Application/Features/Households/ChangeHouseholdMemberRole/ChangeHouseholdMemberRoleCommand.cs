namespace FinanceOS.Identity.Application.Features.Households.ChangeHouseholdMemberRole;

public sealed record ChangeHouseholdMemberRoleCommand(
    Guid HouseholdId,
    Guid ActorUserId,
    Guid UserId,
    string Role);
