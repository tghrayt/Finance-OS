using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Finance.Infrastructure.Persistence;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).HasConversion(id => id.Value, value => new AccountId(value)).ValueGeneratedNever().HasColumnName("id");
        builder.Property(account => account.HouseholdId).HasConversion(id => id.Value, value => new HouseholdId(value)).HasColumnName("household_id").IsRequired();
        builder.Property(account => account.Name).HasMaxLength(160).HasColumnName("name").IsRequired();
        builder.Property(account => account.Type).HasConversion<string>().HasMaxLength(32).HasColumnName("type").IsRequired();
        builder.Property(account => account.Currency).HasMaxLength(3).HasColumnName("currency").IsRequired();
        builder.Property(account => account.InitialBalance).HasColumnType("numeric(18,2)").HasColumnName("initial_balance").IsRequired();
        builder.Property(account => account.CurrentBalance).HasColumnType("numeric(18,2)").HasColumnName("current_balance").IsRequired();
        builder.Property(account => account.InstitutionName).HasMaxLength(160).HasColumnName("institution_name").IsRequired();
        builder.Property(account => account.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(account => account.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(account => account.HouseholdId);
    }
}
