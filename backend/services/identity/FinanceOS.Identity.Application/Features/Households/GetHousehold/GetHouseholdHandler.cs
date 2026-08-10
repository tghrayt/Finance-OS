using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Domain.Households;

namespace FinanceOS.Identity.Application.Features.Households.GetHousehold;

public sealed class GetHouseholdHandler(IHouseholdRepository households)
{
    public async Task<HouseholdDetailsResult> HandleAsync(Guid householdId, CancellationToken cancellationToken)
    {
        if (householdId == Guid.Empty)
        {
            throw new IdentityValidationException("Household id is required.");
        }

        var household = await households.GetByIdAsync(new HouseholdId(householdId), cancellationToken);

        if (household is null)
        {
            throw new IdentityNotFoundException("Household was not found.");
        }

        return HouseholdDetailsResult.FromHousehold(household);
    }
}
