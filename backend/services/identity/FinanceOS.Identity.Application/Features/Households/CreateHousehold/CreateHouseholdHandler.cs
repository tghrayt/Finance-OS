using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Households.CreateHousehold;

public sealed class CreateHouseholdHandler(
    IUserRepository users,
    IHouseholdRepository households,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<CreateHouseholdResult> HandleAsync(
        CreateHouseholdCommand command,
        CancellationToken cancellationToken)
    {
        if (command.OwnerUserId == Guid.Empty)
        {
            throw new IdentityValidationException("Owner user id is required.");
        }

        var ownerId = new UserId(command.OwnerUserId);
        var owner = await users.GetByIdAsync(ownerId, cancellationToken);

        if (owner is null)
        {
            throw new IdentityNotFoundException("Owner user was not found.");
        }

        var household = Household.Create(command.Name, command.Currency, ownerId);

        await households.AddAsync(household, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateHouseholdResult(
            household.Id.Value,
            household.Name,
            household.Currency,
            household.OwnerId.Value,
            household.CreatedAt);
    }
}
