namespace FinanceOS.Finance.Application.Features.Accounts.CreateAccount;

public sealed record CreateAccountCommand(
    Guid HouseholdId,
    string Name,
    string Type,
    decimal InitialBalance,
    string Currency,
    string? InstitutionName);
