using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Domain.Categories;

public sealed class Category
{
    private Category()
    {
        Name = string.Empty;
        Icon = string.Empty;
    }

    private Category(
        CategoryId id,
        HouseholdId householdId,
        string name,
        CategoryId? parentCategoryId,
        string icon,
        bool isSystem,
        DateTimeOffset createdAt)
    {
        Id = id;
        HouseholdId = householdId;
        Name = name;
        ParentCategoryId = parentCategoryId;
        Icon = icon;
        IsSystem = isSystem;
        CreatedAt = createdAt;
    }

    public CategoryId Id { get; }
    public HouseholdId HouseholdId { get; }
    public string Name { get; private set; }
    public CategoryId? ParentCategoryId { get; private set; }
    public string Icon { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static Category Create(
        HouseholdId householdId,
        string name,
        CategoryId? parentCategoryId = null,
        string? icon = null,
        bool isSystem = false,
        DateTimeOffset? createdAt = null)
    {
        if (householdId.Value == Guid.Empty)
        {
            throw new InvalidCategoryException("Household is required.");
        }

        return new Category(
            CategoryId.New(),
            householdId,
            RequiredName(name),
            parentCategoryId,
            icon?.Trim() ?? string.Empty,
            isSystem,
            createdAt ?? SystemClock.UtcNow);
    }

    public void UpdateDetails(string name, string? icon)
    {
        Name = RequiredName(name);
        Icon = icon?.Trim() ?? string.Empty;
    }

    private static string RequiredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidCategoryException("Category name is required.");
        }

        return name.Trim();
    }
}

public sealed class InvalidCategoryException(string message) : DomainException(message);
