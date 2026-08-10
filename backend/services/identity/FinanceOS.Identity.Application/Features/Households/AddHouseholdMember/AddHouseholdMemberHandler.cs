using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Households.AddHouseholdMember;

public sealed class AddHouseholdMemberHandler(
    IUserRepository users,
    IHouseholdRepository households,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<HouseholdDetailsResult> HandleAsync(
        AddHouseholdMemberCommand command,
        CancellationToken cancellationToken)
    {
        if (command.HouseholdId == Guid.Empty || command.ActorUserId == Guid.Empty || command.UserId == Guid.Empty)
        {
            throw new IdentityValidationException("Household id, actor user id and user id are required.");
        }

        var household = await households.GetByIdAsync(new HouseholdId(command.HouseholdId), cancellationToken);
        if (household is null)
        {
            throw new IdentityNotFoundException("Household was not found.");
        }

        if (!household.CanManageMembers(new UserId(command.ActorUserId)))
        {
            throw new IdentityForbiddenException("User cannot manage household members.");
        }

        var userId = new UserId(command.UserId);
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new IdentityNotFoundException("User was not found.");
        }

        household.AddMember(userId, ParseRole(command.Role));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HouseholdDetailsResult.FromHousehold(household);
    }

    private static HouseholdRole ParseRole(string role)
    {
        if (!Enum.TryParse<HouseholdRole>(role, ignoreCase: true, out var parsedRole))
        {
            throw new IdentityValidationException("Household role is invalid.");
        }

        return parsedRole;
    }
}
