using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Users.GetUser;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Users.UpdateUserProfile;

public sealed class UpdateUserProfileHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<UserDetailsResult> HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new IdentityValidationException("User id is required.");
        }

        var user = await users.GetByIdAsync(new UserId(command.UserId), cancellationToken);

        if (user is null)
        {
            throw new IdentityNotFoundException("User was not found.");
        }

        user.UpdateProfile(
            command.FirstName,
            command.LastName,
            command.PreferredCurrency,
            command.Language,
            command.TimeZone);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDetailsResult.FromUser(user);
    }
}
