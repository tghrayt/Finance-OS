namespace FinanceOS.Identity.Application.Features.Users.CreateUser;

public sealed record CreateUserResult(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string PreferredCurrency,
    string Language,
    string TimeZone,
    DateTimeOffset CreatedAt);
