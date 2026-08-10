namespace FinanceOS.Identity.Application.Abstractions;

public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
