using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Abstractions;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetByIdAsync(AccountId id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Account>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken);
}
