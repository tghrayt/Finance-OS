using FinanceOS.Identity.Domain.Common;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Identity.Domain.Households;

public sealed class Household
{
    private readonly List<HouseholdMembership> _memberships = [];

    private Household()
    {
        Name = string.Empty;
        Currency = "EUR";
    }

    private Household(
        HouseholdId id,
        string name,
        string currency,
        UserId ownerId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Currency = currency;
        OwnerId = ownerId;
        CreatedAt = createdAt;
        _memberships.Add(new HouseholdMembership(ownerId, HouseholdRole.Owner, createdAt));
    }

    public HouseholdId Id { get; }

    public string Name { get; private set; }

    public string Currency { get; private set; }

    public UserId OwnerId { get; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<HouseholdMembership> Memberships => _memberships.AsReadOnly();

    public static Household Create(string name, string currency, UserId ownerId, DateTimeOffset? createdAt = null)
    {
        if (ownerId.Value == Guid.Empty)
        {
            throw new InvalidHouseholdException("Owner is required.");
        }

        var timestamp = createdAt ?? SystemClock.UtcNow;

        return new Household(
            HouseholdId.New(),
            RequiredName(name),
            CurrencyCode.Create(currency).Value,
            ownerId,
            timestamp);
    }

    public void Rename(string name)
    {
        Name = RequiredName(name);
    }

    public void AddMember(UserId userId, HouseholdRole role, DateTimeOffset? joinedAt = null)
    {
        if (userId.Value == Guid.Empty)
        {
            throw new InvalidHouseholdMembershipException("User is required.");
        }

        if (role == HouseholdRole.Owner)
        {
            throw new InvalidHouseholdMembershipException("Use ownership transfer to assign a new owner.");
        }

        if (_memberships.Any(membership => membership.UserId == userId))
        {
            throw new InvalidHouseholdMembershipException("User is already a household member.");
        }

        _memberships.Add(new HouseholdMembership(userId, role, joinedAt ?? SystemClock.UtcNow));
    }

    public void ChangeMemberRole(UserId userId, HouseholdRole role)
    {
        var membership = FindMembership(userId);

        if (membership.Role == HouseholdRole.Owner)
        {
            throw new InvalidHouseholdMembershipException("Owner role cannot be changed without ownership transfer.");
        }

        if (role == HouseholdRole.Owner)
        {
            throw new InvalidHouseholdMembershipException("Use ownership transfer to assign a new owner.");
        }

        membership.ChangeRole(role);
    }

    public void RemoveMember(UserId userId)
    {
        var membership = FindMembership(userId);

        if (membership.Role == HouseholdRole.Owner)
        {
            throw new InvalidHouseholdMembershipException("Owner cannot be removed from the household.");
        }

        _memberships.Remove(membership);
    }

    private HouseholdMembership FindMembership(UserId userId)
    {
        return _memberships.FirstOrDefault(membership => membership.UserId == userId)
            ?? throw new InvalidHouseholdMembershipException("User is not a household member.");
    }

    private static string RequiredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidHouseholdException("Household name is required.");
        }

        return name.Trim();
    }
}

public sealed class InvalidHouseholdException(string message) : DomainException(message);

public sealed class InvalidHouseholdMembershipException(string message) : DomainException(message);
