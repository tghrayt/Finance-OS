using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceOS.Budget.Infrastructure.Persistence;

public sealed class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
{
    public BudgetDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BudgetDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=financeos_budget;Username=financeos;Password=financeos")
            .Options;

        return new BudgetDbContext(options);
    }
}
