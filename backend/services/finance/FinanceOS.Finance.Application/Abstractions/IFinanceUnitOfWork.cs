namespace FinanceOS.Finance.Application.Abstractions;

public interface IFinanceUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
