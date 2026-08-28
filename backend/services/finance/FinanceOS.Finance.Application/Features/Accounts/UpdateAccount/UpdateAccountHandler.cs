using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Common;
using FinanceOS.Finance.Application.Features.Accounts.CreateAccount;
using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Accounts.UpdateAccount;

public sealed class UpdateAccountHandler(IAccountRepository accounts, IFinanceUnitOfWork unitOfWork)
{
    public async Task<AccountResult> HandleAsync(UpdateAccountCommand command, CancellationToken cancellationToken)
    {
        if (command.HouseholdId == Guid.Empty || command.AccountId == Guid.Empty)
        {
            throw new FinanceValidationException("Household and account are required.");
        }

        var account = await accounts.GetByIdAsync(new AccountId(command.AccountId), cancellationToken);

        if (account is null || account.HouseholdId != new HouseholdId(command.HouseholdId))
        {
            throw new FinanceNotFoundException("Account was not found.");
        }

        account.UpdateDetails(command.Name, command.Type, command.InstitutionName);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountResult.FromAccount(account);
    }
}
