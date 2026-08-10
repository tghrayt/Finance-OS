using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;

namespace FinanceOS.Foundation.Tests;

public sealed class FinanceDomainTests
{
    [Fact]
    public void MoneyRejectsNegativeAmounts()
    {
        Assert.Throws<InvalidMoneyException>(() => Money.Create(-1, "EUR"));
    }

    [Fact]
    public void TransactionRequiresPositiveAmount()
    {
        Assert.Throws<InvalidMoneyException>(() =>
            FinancialTransaction.Create(HouseholdId.New(), AccountId.New(), TransactionType.Expense, 0, "EUR", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void TransferRequiresDifferentDestinationAccount()
    {
        var accountId = AccountId.New();

        Assert.Throws<InvalidTransactionException>(() =>
            FinancialTransaction.Create(HouseholdId.New(), accountId, TransactionType.Transfer, 10, "EUR", null, null, null, DateOnly.FromDateTime(DateTime.UtcNow), accountId));
    }

    [Fact]
    public void AccountAppliesIncomeAndExpenseImpacts()
    {
        var account = Account.Create(HouseholdId.New(), "Compte courant", AccountType.Checking, 100, "EUR");

        account.Apply(TransactionImpact.Credit, Money.Positive(50, "EUR"));
        account.Apply(TransactionImpact.Debit, Money.Positive(30, "EUR"));

        Assert.Equal(120, account.CurrentBalance);
    }
}
