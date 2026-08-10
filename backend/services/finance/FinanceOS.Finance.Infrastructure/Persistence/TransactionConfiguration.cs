using FinanceOS.Finance.Domain.Accounts;
using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;
using FinanceOS.Finance.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Finance.Infrastructure.Persistence;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).HasConversion(id => id.Value, value => new TransactionId(value)).ValueGeneratedNever().HasColumnName("id");
        builder.Property(transaction => transaction.HouseholdId).HasConversion(id => id.Value, value => new HouseholdId(value)).HasColumnName("household_id").IsRequired();
        builder.Property(transaction => transaction.AccountId).HasConversion(id => id.Value, value => new AccountId(value)).HasColumnName("account_id").IsRequired();
        builder.Property(transaction => transaction.DestinationAccountId).HasConversion(id => id!.Value.Value, value => new AccountId(value)).HasColumnName("destination_account_id");
        builder.Property(transaction => transaction.Type).HasConversion<string>().HasMaxLength(32).HasColumnName("type").IsRequired();
        builder.Property(transaction => transaction.Amount).HasColumnType("numeric(18,2)").HasColumnName("amount").IsRequired();
        builder.Property(transaction => transaction.Currency).HasMaxLength(3).HasColumnName("currency").IsRequired();
        builder.Property(transaction => transaction.CategoryId).HasConversion(id => id!.Value.Value, value => new CategoryId(value)).HasColumnName("category_id");
        builder.Property(transaction => transaction.Merchant).HasMaxLength(160).HasColumnName("merchant").IsRequired();
        builder.Property(transaction => transaction.Description).HasMaxLength(500).HasColumnName("description").IsRequired();
        builder.Property(transaction => transaction.TransactionDate).HasColumnName("transaction_date").IsRequired();
        builder.Property(transaction => transaction.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(transaction => new { transaction.HouseholdId, transaction.TransactionDate });
    }
}
