using FinanceOS.Identity.Domain.Common;

namespace FinanceOS.Identity.Domain.Users;

public sealed record CurrencyCode
{
    private CurrencyCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CurrencyCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidCurrencyCodeException("Currency is required.");
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || normalized.Any(character => character < 'A' || character > 'Z'))
        {
            throw new InvalidCurrencyCodeException("Currency must be a three-letter ISO code.");
        }

        return new CurrencyCode(normalized);
    }

    public override string ToString() => Value;
}

public sealed class InvalidCurrencyCodeException(string message) : DomainException(message);
