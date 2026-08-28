using FinanceOS.Finance.Domain.Accounts;

namespace FinanceOS.Finance.Application.Features.Accounts.UpdateAccount;

public sealed record UpdateAccountCommand(
    Guid HouseholdId,
    Guid AccountId,
    string Name,
    AccountType Type,
    string? InstitutionName);
