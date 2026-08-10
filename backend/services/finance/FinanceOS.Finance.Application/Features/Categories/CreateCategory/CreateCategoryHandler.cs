using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryHandler(
    ICategoryRepository categories,
    IFinanceUnitOfWork unitOfWork)
{
    public async Task<CategoryResult> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            new HouseholdId(command.HouseholdId),
            command.Name,
            command.ParentCategoryId is null ? null : new CategoryId(command.ParentCategoryId.Value),
            command.Icon);

        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CategoryResult.FromCategory(category);
    }
}
