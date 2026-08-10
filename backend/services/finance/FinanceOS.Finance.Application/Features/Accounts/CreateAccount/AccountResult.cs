using FinanceOS.Finance.Domain.Accounts;

namespace FinanceOS.Finance.Application.Features.Accounts.CreateAccount;

public sealed record AccountResult(
    Guid AccountId,
    Guid HouseholdId,
    string Name,
    string Type,
    decimal CurrentBalance,
    string Currency,
    bool IsActive)
{
    public static AccountResult FromAccount(Account account) =>
        new(
            account.Id.Value,
            account.HouseholdId.Value,
            account.Name,
            account.Type.ToString(),
            account.CurrentBalance,
            account.Currency,
            account.IsActive);
}
