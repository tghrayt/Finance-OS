using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Features.Users.GetUser;

public sealed record UserDetailsResult(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PreferredCurrency,
    string Language,
    string TimeZone,
    DateTimeOffset CreatedAt)
{
    public static UserDetailsResult FromUser(User user)
    {
        return new UserDetailsResult(
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
