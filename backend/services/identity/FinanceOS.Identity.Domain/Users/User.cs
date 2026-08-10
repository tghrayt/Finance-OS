using FinanceOS.Identity.Domain.Common;

namespace FinanceOS.Identity.Domain.Users;

public sealed class User
{
    private User(
        UserId id,
        string firstName,
        string lastName,
        EmailAddress email,
        string preferredCurrency,
        string language,
        string timeZone,
        DateTimeOffset createdAt)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PreferredCurrency = preferredCurrency;
        Language = language;
        TimeZone = timeZone;
        CreatedAt = createdAt;
    }

    public UserId Id { get; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public EmailAddress Email { get; }

    public string PreferredCurrency { get; private set; }

    public string Language { get; private set; }

    public string TimeZone { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public static User Register(
        string firstName,
        string lastName,
        string email,
        string preferredCurrency,
        string language,
        string timeZone,
        DateTimeOffset? createdAt = null)
    {
        return new User(
            UserId.New(),
            RequiredText(firstName, nameof(firstName)),
            RequiredText(lastName, nameof(lastName)),
            EmailAddress.Create(email),
            CurrencyCode.Create(preferredCurrency).Value,
            RequiredText(language, nameof(language)).ToLowerInvariant(),
            RequiredText(timeZone, nameof(timeZone)),
            createdAt ?? SystemClock.UtcNow);
    }

    public void UpdateProfile(string firstName, string lastName, string preferredCurrency, string language, string timeZone)
    {
        FirstName = RequiredText(firstName, nameof(firstName));
        LastName = RequiredText(lastName, nameof(lastName));
        PreferredCurrency = CurrencyCode.Create(preferredCurrency).Value;
        Language = RequiredText(language, nameof(language)).ToLowerInvariant();
        TimeZone = RequiredText(timeZone, nameof(timeZone));
    }

    private static string RequiredText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidUserProfileException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}

public sealed class InvalidUserProfileException(string message) : DomainException(message);
