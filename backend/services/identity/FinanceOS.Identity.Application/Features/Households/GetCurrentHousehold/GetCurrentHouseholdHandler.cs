using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;

public sealed class GetCurrentHouseholdHandler(IHouseholdRepository households)
{
    public async Task<HouseholdDetailsResult> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new IdentityValidationException("User id is required.");
        }

        var household = await households.GetFirstByMemberAsync(new UserId(userId), cancellationToken);

        if (household is null)
        {
            throw new IdentityNotFoundException("Household was not found.");
        }

        return HouseholdDetailsResult.FromHousehold(household);
    }
}
