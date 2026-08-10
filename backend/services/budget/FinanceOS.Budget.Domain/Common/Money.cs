namespace FinanceOS.Budget.Domain.Common;

public sealed record Money
{
    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public CurrencyCode Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new InvalidMoneyException("Money amount cannot be negative.");
        }

        return new Money(decimal.Round(amount, 2), CurrencyCode.Create(currency));
    }

    public static Money Positive(decimal amount, string currency)
    {
        if (amount <= 0)
        {
            throw new InvalidMoneyException("Amount must be strictly positive.");
        }

        return Create(amount, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency.Value);
    }

    public decimal RatioOf(Money total)
    {
        EnsureSameCurrency(total);
        return total.Amount == 0 ? 0 : Amount / total.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidMoneyException("Money currencies must match.");
        }
    }
}

public sealed class InvalidMoneyException(string message) : DomainException(message);
