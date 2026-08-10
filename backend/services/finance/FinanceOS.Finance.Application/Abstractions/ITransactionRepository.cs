using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;

namespace FinanceOS.Finance.Application.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<FinancialTransaction>> ListByHouseholdAsync(HouseholdId householdId, int page, int pageSize, CancellationToken cancellationToken);
}
