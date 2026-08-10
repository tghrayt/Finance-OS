using FinanceOS.Finance.Domain.Categories;

namespace FinanceOS.Finance.Application.Features.Categories.CreateCategory;

public sealed record CategoryResult(
    Guid CategoryId,
    Guid HouseholdId,
    string Name,
    Guid? ParentCategoryId,
    string Icon,
    bool IsSystem)
{
    public static CategoryResult FromCategory(Category category) =>
        new(
            category.Id.Value,
            category.HouseholdId.Value,
            category.Name,
            category.ParentCategoryId?.Value,
            category.Icon,
            category.IsSystem);
}
