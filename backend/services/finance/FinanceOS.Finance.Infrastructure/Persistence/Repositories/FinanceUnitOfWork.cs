using FinanceOS.Finance.Application.Abstractions;

namespace FinanceOS.Finance.Infrastructure.Persistence.Repositories;

internal sealed class FinanceUnitOfWork(FinanceDbContext dbContext) : IFinanceUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken) => await dbContext.SaveChangesAsync(cancellationToken);
}
