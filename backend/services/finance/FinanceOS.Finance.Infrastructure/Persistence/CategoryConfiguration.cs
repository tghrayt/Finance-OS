using FinanceOS.Finance.Domain.Categories;
using FinanceOS.Finance.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceOS.Finance.Infrastructure.Persistence;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).HasConversion(id => id.Value, value => new CategoryId(value)).ValueGeneratedNever().HasColumnName("id");
        builder.Property(category => category.HouseholdId).HasConversion(id => id.Value, value => new HouseholdId(value)).HasColumnName("household_id").IsRequired();
        builder.Property(category => category.Name).HasMaxLength(120).HasColumnName("name").IsRequired();
        builder.Property(category => category.ParentCategoryId).HasConversion(id => id!.Value.Value, value => new CategoryId(value)).HasColumnName("parent_category_id");
        builder.Property(category => category.Icon).HasMaxLength(64).HasColumnName("icon").IsRequired();
        builder.Property(category => category.IsSystem).HasColumnName("is_system").IsRequired();
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(category => category.HouseholdId);
    }
}
