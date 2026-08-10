using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Identity.Infrastructure.Persistence.Repositories;

internal sealed class HouseholdRepository(IdentityDbContext dbContext) : IHouseholdRepository
{
    public async Task AddAsync(Household household, CancellationToken cancellationToken)
    {
        await dbContext.Households.AddAsync(household, cancellationToken);
    }

    public async Task<Household?> GetByIdAsync(HouseholdId id, CancellationToken cancellationToken)
    {
        return await dbContext.Households
            .Include(household => household.Memberships)
            .FirstOrDefaultAsync(household => household.Id == id, cancellationToken);
    }

    public async Task<Household?> GetFirstByMemberAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await dbContext.Households
            .Include(household => household.Memberships)
            .Where(household => household.Memberships.Any(membership => membership.UserId == userId))
            .OrderBy(household => household.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
