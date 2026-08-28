namespace FinanceOS.Finance.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid HouseholdId, Guid CategoryId, string Name, string? Icon);
