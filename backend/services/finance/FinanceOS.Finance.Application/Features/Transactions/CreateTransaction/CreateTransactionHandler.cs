using FinanceOS.Contracts.Finance;
using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Common;
using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;

namespace FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;

public sealed class CreateTransactionHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IOutboxWriter outbox,
    IFinanceUnitOfWork unitOfWork)
{
    public async Task<TransactionResult> HandleAsync(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(command.Type, ignoreCase: true, out var type))
        {
            throw new FinanceValidationException("Transaction type is invalid.");
        }

        var householdId = new HouseholdId(command.HouseholdId);
        var source = await accounts.GetByIdAsync(new AccountId(command.AccountId), cancellationToken)
            ?? throw new FinanceNotFoundException("Source account was not found.");

        if (source.HouseholdId != householdId)
        {
            throw new FinanceValidationException("Source account does not belong to the household.");
        }

        AccountId? destinationId = command.DestinationAccountId is null
            ? null
            : new AccountId(command.DestinationAccountId.Value);
        Account? destination = null;
        if (destinationId is not null)
        {
            destination = await accounts.GetByIdAsync(destinationId.Value, cancellationToken)
                ?? throw new FinanceNotFoundException("Destination account was not found.");

            if (destination.HouseholdId != householdId)
            {
                throw new FinanceValidationException("Destination account does not belong to the household.");
            }
        }

        var transaction = FinancialTransaction.Create(
            householdId,
            source.Id,
            type,
            command.Amount,
            command.Currency,
            command.CategoryId is null ? null : new CategoryId(command.CategoryId.Value),
            command.Merchant,
            command.Description,
            command.TransactionDate,
            destinationId);

        var amount = Money.Positive(transaction.Amount, transaction.Currency);
        source.Apply(transaction.SourceImpact, amount);
        if (transaction.Type == TransactionType.Transfer && destination is not null)
        {
            destination.Apply(TransactionImpact.Credit, amount);
        }

        await transactions.AddAsync(transaction, cancellationToken);
        outbox.Add(new TransactionCreatedV1(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            command.CorrelationId == Guid.Empty ? Guid.NewGuid() : command.CorrelationId,
            transaction.Id.Value,
            transaction.HouseholdId.Value,
            transaction.AccountId.Value,
            transaction.DestinationAccountId?.Value,
            transaction.Type.ToString(),
            transaction.Amount,
            transaction.Currency,
            transaction.CategoryId?.Value,
            transaction.TransactionDate));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TransactionResult.FromTransaction(transaction);
    }
}
