using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Features.Categories.CreateCategory;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesHandler(ICategoryRepository categories)
{
    public async Task<IReadOnlyCollection<CategoryResult>> HandleAsync(Guid householdId, CancellationToken cancellationToken)
    {
        var result = await categories.ListByHouseholdAsync(new HouseholdId(householdId), cancellationToken);
        return result.Select(CategoryResult.FromCategory).ToArray();
    }
}
