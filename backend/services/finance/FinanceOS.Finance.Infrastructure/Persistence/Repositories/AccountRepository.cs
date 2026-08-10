using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Finance.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(FinanceDbContext dbContext) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken cancellationToken) => await dbContext.Accounts.AddAsync(account, cancellationToken);

    public async Task<Account?> GetByIdAsync(AccountId id, CancellationToken cancellationToken) =>
        await dbContext.Accounts.FirstOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Account>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.AsNoTracking().Where(account => account.HouseholdId == householdId).OrderBy(account => account.Name).ToArrayAsync(cancellationToken);
}
