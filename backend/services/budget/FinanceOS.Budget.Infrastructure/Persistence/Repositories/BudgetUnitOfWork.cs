using FinanceOS.Budget.Application.Abstractions;

namespace FinanceOS.Budget.Infrastructure.Persistence.Repositories;

internal sealed class BudgetUnitOfWork(BudgetDbContext dbContext) : IBudgetUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken) => await dbContext.SaveChangesAsync(cancellationToken);
}
