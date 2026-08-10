using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Application.Abstractions;

public interface IHouseholdRepository
{
    Task AddAsync(Household household, CancellationToken cancellationToken);

    Task<Household?> GetByIdAsync(HouseholdId id, CancellationToken cancellationToken);

    Task<Household?> GetFirstByMemberAsync(UserId userId, CancellationToken cancellationToken);
}
