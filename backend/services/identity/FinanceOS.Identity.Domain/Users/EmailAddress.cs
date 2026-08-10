using FinanceOS.Identity.Domain.Common;

namespace FinanceOS.Identity.Domain.Users;

public sealed record EmailAddress
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEmailAddressException("Email is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (!normalized.Contains('@', StringComparison.Ordinal) || normalized.Length > 320)
        {
            throw new InvalidEmailAddressException("Email format is invalid.");
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}

public sealed class InvalidEmailAddressException(string message) : DomainException(message);
