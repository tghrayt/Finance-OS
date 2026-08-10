namespace FinanceOS.Identity.Application.Features.Users.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string PreferredCurrency,
    string Language,
    string TimeZone);
