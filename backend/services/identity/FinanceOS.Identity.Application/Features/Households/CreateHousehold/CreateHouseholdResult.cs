namespace FinanceOS.Identity.Application.Features.Households.CreateHousehold;

public sealed record CreateHouseholdResult(
    Guid HouseholdId,
    string Name,
    string Currency,
    Guid OwnerUserId,
    DateTimeOffset CreatedAt);
