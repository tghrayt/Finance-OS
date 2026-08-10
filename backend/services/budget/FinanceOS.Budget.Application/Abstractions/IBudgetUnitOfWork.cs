namespace FinanceOS.Budget.Application.Abstractions;

public interface IBudgetUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
