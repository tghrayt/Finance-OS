using FinanceOS.Budget.Domain.Budgets;
using Microsoft.EntityFrameworkCore;

namespace FinanceOS.Budget.Infrastructure.Persistence;

public sealed class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
{
    public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("budget");
        modelBuilder.ApplyConfiguration(new MonthlyBudgetConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}
