namespace FinanceOS.Identity.Application.Features.Households.CreateHousehold;

public sealed record CreateHouseholdCommand(
    Guid OwnerUserId,
    string Name,
    string Currency);
