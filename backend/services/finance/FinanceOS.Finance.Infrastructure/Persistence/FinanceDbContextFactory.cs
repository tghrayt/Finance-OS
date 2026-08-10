using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceOS.Finance.Infrastructure.Persistence;

public sealed class FinanceDbContextFactory : IDesignTimeDbContextFactory<FinanceDbContext>
{
    public FinanceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=financeos;Username=financeos;Password=financeos")
            .Options;

        return new FinanceDbContext(options);
    }
}
