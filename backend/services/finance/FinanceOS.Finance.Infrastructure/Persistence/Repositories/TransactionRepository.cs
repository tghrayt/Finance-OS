using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Finance.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository(FinanceDbContext dbContext) : ITransactionRepository
{
    public async Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken) => await dbContext.Transactions.AddAsync(transaction, cancellationToken);

    public async Task<IReadOnlyCollection<FinancialTransaction>> ListByHouseholdAsync(HouseholdId householdId, int page, int pageSize, CancellationToken cancellationToken) =>
        await dbContext.Transactions.AsNoTracking()
            .Where(transaction => transaction.HouseholdId == householdId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
}
