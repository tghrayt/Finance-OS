using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Household> Households => Set<Household>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new HouseholdConfiguration());
    }
}
