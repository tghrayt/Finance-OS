namespace FinanceOS.Finance.Application.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(
    Guid HouseholdId,
    string Name,
    Guid? ParentCategoryId,
    string? Icon);
