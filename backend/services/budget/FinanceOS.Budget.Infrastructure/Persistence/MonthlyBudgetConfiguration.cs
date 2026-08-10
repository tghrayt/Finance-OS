using FinanceOS.Budget.Domain.Budgets;
using FinanceOS.Budget.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Budget.Infrastructure.Persistence;

internal sealed class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        builder.ToTable("monthly_budgets");
        builder.HasKey(budget => budget.Id);
        builder.Property(budget => budget.Id).HasConversion(id => id.Value, value => new MonthlyBudgetId(value)).ValueGeneratedNever().HasColumnName("id");
        builder.Property(budget => budget.HouseholdId).HasConversion(id => id.Value, value => new HouseholdId(value)).HasColumnName("household_id").IsRequired();
        builder.Property(budget => budget.Year).HasColumnName("year").IsRequired();
        builder.Property(budget => budget.Month).HasColumnName("month").IsRequired();
        builder.Property(budget => budget.TotalBudget).HasPrecision(18, 2).HasColumnName("total_budget").IsRequired();
        builder.Property(budget => budget.Currency).HasMaxLength(3).HasColumnName("currency").IsRequired();
        builder.Property(budget => budget.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Ignore(budget => budget.ActualAmount);
        builder.Ignore(budget => budget.ConsumptionRatio);
        builder.HasIndex(budget => new { budget.HouseholdId, budget.Year, budget.Month }).IsUnique();

        builder.Navigation(budget => budget.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(
            budget => budget.Allocations,
            allocation =>
            {
                allocation.ToTable("budget_allocations");
                allocation.WithOwner().HasForeignKey("budget_id");
                allocation.HasKey(item => item.Id);
                allocation.Property(item => item.Id).HasConversion(id => id.Value, value => new BudgetAllocationId(value)).ValueGeneratedNever().HasColumnName("id");
                allocation.Property(item => item.CategoryId).HasColumnName("category_id").IsRequired();
                allocation.Property(item => item.PlannedAmount).HasPrecision(18, 2).HasColumnName("planned_amount").IsRequired();
                allocation.Property(item => item.ActualAmount).HasPrecision(18, 2).HasColumnName("actual_amount").IsRequired();
                allocation.Property(item => item.Currency).HasMaxLength(3).HasColumnName("currency").IsRequired();
                allocation.Ignore(item => item.ConsumptionRatio);
                allocation.HasIndex("budget_id", nameof(BudgetAllocation.CategoryId)).IsUnique();
            });
    }
}
