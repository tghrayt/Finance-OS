using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Households.RemoveHouseholdMember;

public sealed class RemoveHouseholdMemberHandler(
    IHouseholdRepository households,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<HouseholdDetailsResult> HandleAsync(
        RemoveHouseholdMemberCommand command,
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

        household.RemoveMember(new UserId(command.UserId));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HouseholdDetailsResult.FromHousehold(household);
    }
}
