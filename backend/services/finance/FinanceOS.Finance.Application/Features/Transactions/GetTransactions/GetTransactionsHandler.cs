using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Transactions.GetTransactions;

public sealed class GetTransactionsHandler(ITransactionRepository transactions)
{
    public async Task<IReadOnlyCollection<TransactionResult>> HandleAsync(
        Guid householdId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var result = await transactions.ListByHouseholdAsync(new HouseholdId(householdId), safePage, safePageSize, cancellationToken);
        return result.Select(TransactionResult.FromTransaction).ToArray();
    }
}
