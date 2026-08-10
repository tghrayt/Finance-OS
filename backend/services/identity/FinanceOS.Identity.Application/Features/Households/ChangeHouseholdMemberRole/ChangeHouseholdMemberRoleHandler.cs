using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Households.ChangeHouseholdMemberRole;

public sealed class ChangeHouseholdMemberRoleHandler(
    IHouseholdRepository households,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<HouseholdDetailsResult> HandleAsync(
        ChangeHouseholdMemberRoleCommand command,
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

        household.ChangeMemberRole(new UserId(command.UserId), ParseRole(command.Role));

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
