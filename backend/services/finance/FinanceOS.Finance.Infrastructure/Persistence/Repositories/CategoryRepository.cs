using FinanceOS.Finance.Application.Abstractions;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Finance.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(FinanceDbContext dbContext) : ICategoryRepository
{
    public async Task AddAsync(Category category, CancellationToken cancellationToken) => await dbContext.Categories.AddAsync(category, cancellationToken);

    public async Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken) =>
        await dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Category>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken cancellationToken) =>
        await dbContext.Categories.AsNoTracking().Where(category => category.HouseholdId == householdId).OrderBy(category => category.Name).ToArrayAsync(cancellationToken);
}
