using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Features.Accounts.CreateAccount;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Accounts.GetAccounts;

public sealed class GetAccountsHandler(IAccountRepository accounts)
{
    public async Task<IReadOnlyCollection<AccountResult>> HandleAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var result = await accounts.ListByHouseholdAsync(new HouseholdId(householdId), cancellationToken);
        return result.Select(AccountResult.FromAccount).ToArray();
    }
}
