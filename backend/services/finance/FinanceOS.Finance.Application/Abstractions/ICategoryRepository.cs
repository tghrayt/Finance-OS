using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Application.Abstractions;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Category>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken);
}
