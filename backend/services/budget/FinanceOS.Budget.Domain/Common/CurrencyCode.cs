namespace FinanceOS.Budget.Domain.Common;

public readonly record struct CurrencyCode
{
    private CurrencyCode(string value) => Value = value;

    public string Value { get; }

    public static CurrencyCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length != 3)
        {
            throw new InvalidCurrencyException("Currency must be a 3-letter ISO code.");
        }

        return new CurrencyCode(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}

public sealed class InvalidCurrencyException(string message) : DomainException(message);
