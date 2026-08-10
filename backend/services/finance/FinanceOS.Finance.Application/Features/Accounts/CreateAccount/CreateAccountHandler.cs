using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Common;
using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Accounts.CreateAccount;

public sealed class CreateAccountHandler(
    IAccountRepository accounts,
    IFinanceUnitOfWork unitOfWork)
{
    public async Task<AccountResult> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountType>(command.Type, ignoreCase: true, out var type))
        {
            throw new FinanceValidationException("Account type is invalid.");
        }

        var account = Account.Create(
            new HouseholdId(command.HouseholdId),
            command.Name,
            type,
            command.InitialBalance,
            command.Currency,
            command.InstitutionName);

        await accounts.AddAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResult.FromAccount(account);
    }
}
