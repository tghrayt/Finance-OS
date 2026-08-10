using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Users.CreateUser;

public sealed class CreateUserHandler(
    IUserRepository users,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<CreateUserResult> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email);

        if (await users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new IdentityConflictException("A user with this email already exists.");
        }

        var user = User.Register(
            command.FirstName,
            command.LastName,
            email.Value,
            command.PreferredCurrency,
            command.Language,
            command.TimeZone);

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(
            user.Id.Value,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            user.PreferredCurrency,
            user.Language,
            user.TimeZone,
            user.CreatedAt);
    }
}
