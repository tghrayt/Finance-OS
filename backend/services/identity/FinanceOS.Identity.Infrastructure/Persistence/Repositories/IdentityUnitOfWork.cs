using FinanceOS.Identity.Application.Abstractions;

namespace FinanceOS.Identity.Infrastructure.Persistence.Repositories;

internal sealed class IdentityUnitOfWork(IdentityDbContext dbContext) : IIdentityUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
