using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Application.Common;
using FinanceOS.Finance.Application.Features.Categories.CreateCategory;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler(ICategoryRepository categories, IFinanceUnitOfWork unitOfWork)
{
    public async Task<CategoryResult> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (command.HouseholdId == Guid.Empty || command.CategoryId == Guid.Empty)
        {
            throw new FinanceValidationException("Household and category are required.");
        }

        var category = await categories.GetByIdAsync(new CategoryId(command.CategoryId), cancellationToken);

        if (category is null || category.HouseholdId != new HouseholdId(command.HouseholdId))
        {
            throw new FinanceNotFoundException("Category was not found.");
        }

        category.UpdateDetails(command.Name, command.Icon);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CategoryResult.FromCategory(category);
    }
}
