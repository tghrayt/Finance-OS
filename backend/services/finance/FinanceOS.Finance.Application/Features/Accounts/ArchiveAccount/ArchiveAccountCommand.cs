namespace FinanceOS.Finance.Application.Features.Accounts.ArchiveAccount;

public sealed record ArchiveAccountCommand(
    Guid HouseholdId,
    Guid AccountId);
