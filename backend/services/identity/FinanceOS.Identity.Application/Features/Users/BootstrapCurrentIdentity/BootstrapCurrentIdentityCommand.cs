namespace FinanceOS.Identity.Application.Features.Users.BootstrapCurrentIdentity;

public sealed record BootstrapCurrentIdentityCommand(
    string ExternalSubject,
    string Email,
    string DisplayName,
    string PreferredCurrency,
    string Language,
    string TimeZone);
