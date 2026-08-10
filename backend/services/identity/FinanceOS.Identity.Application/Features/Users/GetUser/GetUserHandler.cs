using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Users.GetUser;

public sealed class GetUserHandler(IUserRepository users)
{
    public async Task<UserDetailsResult> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new IdentityValidationException("User id is required.");
        }

        var user = await users.GetByIdAsync(new UserId(userId), cancellationToken);

        if (user is null)
        {
            throw new IdentityNotFoundException("User was not found.");
        }

        return UserDetailsResult.FromUser(user);
    }
}
