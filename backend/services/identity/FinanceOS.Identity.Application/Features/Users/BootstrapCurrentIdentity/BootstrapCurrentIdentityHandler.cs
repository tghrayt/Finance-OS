using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Users.GetUser;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Users.BootstrapCurrentIdentity;

public sealed class BootstrapCurrentIdentityHandler(
    IUserRepository users,
    IHouseholdRepository households,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<BootstrapCurrentIdentityResult> HandleAsync(
        BootstrapCurrentIdentityCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalSubject))
        {
            throw new IdentityValidationException("External subject is required.");
        }

        var email = EmailAddress.Create(command.Email);
        var user = await users.GetByExternalSubjectAsync(command.ExternalSubject, cancellationToken)
            ?? await users.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            user = User.RegisterExternal(
                command.DisplayName,
                email.Value,
                command.ExternalSubject,
                command.PreferredCurrency,
                command.Language,
                command.TimeZone);

            await users.AddAsync(user, cancellationToken);
        }
        else
        {
            user.LinkExternalSubject(command.ExternalSubject);
        }

        var household = await households.GetFirstByMemberAsync(user.Id, cancellationToken);
        if (household is null)
        {
            household = Household.Create("FinanceOS Household", user.PreferredCurrency, user.Id);
            await households.AddAsync(household, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BootstrapCurrentIdentityResult(
            UserDetailsResult.FromUser(user),
            HouseholdDetailsResult.FromHousehold(household));
    }
}
