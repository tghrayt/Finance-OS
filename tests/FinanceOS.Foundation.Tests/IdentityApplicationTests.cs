using FinanceOS.Identity.Application.Abstractions;
using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.AddHouseholdMember;
using FinanceOS.Identity.Application.Features.Households.ChangeHouseholdMemberRole;
using FinanceOS.Identity.Application.Features.Households.CreateHousehold;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Households.GetHousehold;
using FinanceOS.Identity.Application.Features.Households.RemoveHouseholdMember;
using FinanceOS.Identity.Application.Features.Users.CreateUser;
using FinanceOS.Identity.Application.Features.Users.GetUser;
using FinanceOS.Identity.Application.Features.Users.UpdateUserProfile;
using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Foundation.Tests;

public sealed class IdentityApplicationTests
{
    [Fact]
    public async Task CreateUserPersistsNormalizedUser()
    {
        var users = new InMemoryUserRepository();
        var handler = new CreateUserHandler(users, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new CreateUserCommand(" Ait ", " Tghrayt ", " USER@Example.COM ", " eur ", " FR ", "Europe/Paris"),
            CancellationToken.None);

        Assert.Equal("Ait", result.FirstName);
        Assert.Equal("Tghrayt", result.LastName);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("EUR", result.PreferredCurrency);
        Assert.Equal("fr", result.Language);
        Assert.True(await users.ExistsByEmailAsync(EmailAddress.Create("user@example.com"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateUserRejectsDuplicateEmail()
    {
        var users = new InMemoryUserRepository();
        var handler = new CreateUserHandler(users, new InMemoryUnitOfWork());

        await handler.HandleAsync(
            new CreateUserCommand("Ait", "Tghrayt", "user@example.com", "EUR", "fr", "Europe/Paris"),
            CancellationToken.None);

        await Assert.ThrowsAsync<IdentityConflictException>(() =>
            handler.HandleAsync(
                new CreateUserCommand("Other", "User", "USER@example.com", "EUR", "fr", "Europe/Paris"),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateHouseholdCreatesOwnerMembershipForExistingUser()
    {
        var users = new InMemoryUserRepository();
        var households = new InMemoryHouseholdRepository();
        var user = User.Register("Ait", "Tghrayt", "user@example.com", "EUR", "fr", "Europe/Paris");
        await users.AddAsync(user, CancellationToken.None);

        var handler = new CreateHouseholdHandler(users, households, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new CreateHouseholdCommand(user.Id.Value, "Family", "eur"),
            CancellationToken.None);

        var household = await households.GetByIdAsync(new HouseholdId(result.HouseholdId), CancellationToken.None);

        Assert.NotNull(household);
        Assert.Equal("Family", result.Name);
        Assert.Equal("EUR", result.Currency);
        Assert.Contains(household.Memberships, membership =>
            membership.UserId == user.Id && membership.Role == HouseholdRole.Owner);
    }

    [Fact]
    public async Task CreateHouseholdRejectsMissingOwner()
    {
        var handler = new CreateHouseholdHandler(
            new InMemoryUserRepository(),
            new InMemoryHouseholdRepository(),
            new InMemoryUnitOfWork());

        await Assert.ThrowsAsync<IdentityNotFoundException>(() =>
            handler.HandleAsync(
                new CreateHouseholdCommand(Guid.NewGuid(), "Family", "EUR"),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentHouseholdReturnsFirstHouseholdForMember()
    {
        var users = new InMemoryUserRepository();
        var households = new InMemoryHouseholdRepository();
        var user = User.Register("Ait", "Tghrayt", "user@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", user.Id);

        await users.AddAsync(user, CancellationToken.None);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new GetCurrentHouseholdHandler(households);

        var result = await handler.HandleAsync(user.Id.Value, CancellationToken.None);

        Assert.Equal(household.Id.Value, result.HouseholdId);
        Assert.Single(result.Members);
    }

    [Fact]
    public async Task GetUserReturnsExistingUser()
    {
        var users = new InMemoryUserRepository();
        var user = User.Register("Ait", "Tghrayt", "user@example.com", "EUR", "fr", "Europe/Paris");
        await users.AddAsync(user, CancellationToken.None);

        var handler = new GetUserHandler(users);

        var result = await handler.HandleAsync(user.Id.Value, CancellationToken.None);

        Assert.Equal(user.Id.Value, result.UserId);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateUserProfilePersistsProfileChanges()
    {
        var users = new InMemoryUserRepository();
        var user = User.Register("Ait", "Tghrayt", "user@example.com", "EUR", "fr", "Europe/Paris");
        await users.AddAsync(user, CancellationToken.None);

        var handler = new UpdateUserProfileHandler(users, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new UpdateUserProfileCommand(user.Id.Value, "Youssef", "Tghrayt", "usd", "EN", "UTC"),
            CancellationToken.None);

        Assert.Equal("Youssef", result.FirstName);
        Assert.Equal("USD", result.PreferredCurrency);
        Assert.Equal("en", result.Language);
        Assert.Equal("UTC", result.TimeZone);
    }

    [Fact]
    public async Task GetHouseholdReturnsHouseholdDetails()
    {
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new GetHouseholdHandler(households);

        var result = await handler.HandleAsync(household.Id.Value, CancellationToken.None);

        Assert.Equal(household.Id.Value, result.HouseholdId);
        Assert.Single(result.Members);
    }

    [Fact]
    public async Task AddHouseholdMemberAddsExistingUserAsMember()
    {
        var users = new InMemoryUserRepository();
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var member = User.Register("Other", "User", "member@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);

        await users.AddAsync(owner, CancellationToken.None);
        await users.AddAsync(member, CancellationToken.None);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new AddHouseholdMemberHandler(users, households, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new AddHouseholdMemberCommand(household.Id.Value, owner.Id.Value, member.Id.Value, "Member"),
            CancellationToken.None);

        Assert.Contains(result.Members, membership =>
            membership.UserId == member.Id.Value && membership.Role == "Member");
    }

    [Fact]
    public async Task ChangeHouseholdMemberRoleUpdatesExistingMember()
    {
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var member = User.Register("Other", "User", "member@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);
        household.AddMember(member.Id, HouseholdRole.Member);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new ChangeHouseholdMemberRoleHandler(households, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new ChangeHouseholdMemberRoleCommand(household.Id.Value, owner.Id.Value, member.Id.Value, "Viewer"),
            CancellationToken.None);

        Assert.Contains(result.Members, membership =>
            membership.UserId == member.Id.Value && membership.Role == "Viewer");
    }

    [Fact]
    public async Task RemoveHouseholdMemberRejectsOwnerRemoval()
    {
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new RemoveHouseholdMemberHandler(households, new InMemoryUnitOfWork());

        await Assert.ThrowsAsync<InvalidHouseholdMembershipException>(() =>
            handler.HandleAsync(
                new RemoveHouseholdMemberCommand(household.Id.Value, owner.Id.Value, owner.Id.Value),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveHouseholdMemberRemovesExistingMember()
    {
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var member = User.Register("Other", "User", "member@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);
        household.AddMember(member.Id, HouseholdRole.Member);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new RemoveHouseholdMemberHandler(households, new InMemoryUnitOfWork());

        var result = await handler.HandleAsync(
            new RemoveHouseholdMemberCommand(household.Id.Value, owner.Id.Value, member.Id.Value),
            CancellationToken.None);

        Assert.DoesNotContain(result.Members, membership => membership.UserId == member.Id.Value);
    }

    [Fact]
    public async Task MemberCannotManageHouseholdMembers()
    {
        var users = new InMemoryUserRepository();
        var households = new InMemoryHouseholdRepository();
        var owner = User.Register("Ait", "Tghrayt", "owner@example.com", "EUR", "fr", "Europe/Paris");
        var member = User.Register("Other", "User", "member@example.com", "EUR", "fr", "Europe/Paris");
        var invited = User.Register("Invited", "User", "invited@example.com", "EUR", "fr", "Europe/Paris");
        var household = Household.Create("Family", "EUR", owner.Id);
        household.AddMember(member.Id, HouseholdRole.Member);

        await users.AddAsync(owner, CancellationToken.None);
        await users.AddAsync(member, CancellationToken.None);
        await users.AddAsync(invited, CancellationToken.None);
        await households.AddAsync(household, CancellationToken.None);

        var handler = new AddHouseholdMemberHandler(users, households, new InMemoryUnitOfWork());

        await Assert.ThrowsAsync<IdentityForbiddenException>(() =>
            handler.HandleAsync(
                new AddHouseholdMemberCommand(household.Id.Value, member.Id.Value, invited.Id.Value, "Viewer"),
                CancellationToken.None));
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.FirstOrDefault(user => user.Id == id));
        }

        public Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Any(user => user.Email == email));
        }
    }

    private sealed class InMemoryHouseholdRepository : IHouseholdRepository
    {
        private readonly List<Household> _households = [];

        public Task AddAsync(Household household, CancellationToken cancellationToken)
        {
            _households.Add(household);
            return Task.CompletedTask;
        }

        public Task<Household?> GetByIdAsync(HouseholdId id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_households.FirstOrDefault(household => household.Id == id));
        }

        public Task<Household?> GetFirstByMemberAsync(UserId userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_households
                .Where(household => household.Memberships.Any(membership => membership.UserId == userId))
                .OrderBy(household => household.CreatedAt)
                .FirstOrDefault());
        }
    }

    private sealed class InMemoryUnitOfWork : IIdentityUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
