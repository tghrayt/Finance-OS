using FinanceOS.Identity.Domain.Households;
using FinanceOS.Identity.Domain.Users;

namespace FinanceOS.Foundation.Tests;

public sealed class IdentityDomainTests
{
    [Fact]
    public void RegisterUserNormalizesEmailLanguageAndCurrency()
    {
        var user = User.Register(
            " Ait ",
            " Tghrayt ",
            " USER@Example.COM ",
            " eur ",
            " FR ",
            "Europe/Paris");

        Assert.Equal("Ait", user.FirstName);
        Assert.Equal("Tghrayt", user.LastName);
        Assert.Equal("user@example.com", user.Email.Value);
        Assert.Equal("EUR", user.PreferredCurrency);
        Assert.Equal("fr", user.Language);
    }

    [Fact]
    public void CreateHouseholdCreatesOwnerMembership()
    {
        var ownerId = UserId.New();

        var household = Household.Create("Family", "EUR", ownerId);

        var membership = Assert.Single(household.Memberships);
        Assert.Equal(ownerId, household.OwnerId);
        Assert.Equal(ownerId, membership.UserId);
        Assert.Equal(HouseholdRole.Owner, membership.Role);
    }

    [Fact]
    public void AddMemberRejectsDuplicateMembership()
    {
        var household = Household.Create("Family", "EUR", UserId.New());
        var memberId = UserId.New();

        household.AddMember(memberId, HouseholdRole.Member);

        Assert.Throws<InvalidHouseholdMembershipException>(() =>
            household.AddMember(memberId, HouseholdRole.Viewer));
    }

    [Fact]
    public void AddMemberRejectsOwnerRole()
    {
        var household = Household.Create("Family", "EUR", UserId.New());

        Assert.Throws<InvalidHouseholdMembershipException>(() =>
            household.AddMember(UserId.New(), HouseholdRole.Owner));
    }

    [Fact]
    public void ChangeMemberRoleRejectsOwnerDemotion()
    {
        var ownerId = UserId.New();
        var household = Household.Create("Family", "EUR", ownerId);

        Assert.Throws<InvalidHouseholdMembershipException>(() =>
            household.ChangeMemberRole(ownerId, HouseholdRole.Admin));
    }

    [Fact]
    public void RemoveMemberRejectsOwnerRemoval()
    {
        var ownerId = UserId.New();
        var household = Household.Create("Family", "EUR", ownerId);

        Assert.Throws<InvalidHouseholdMembershipException>(() =>
            household.RemoveMember(ownerId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void CurrencyRequiresThreeLetterIsoCode(string currency)
    {
        Assert.Throws<InvalidCurrencyCodeException>(() => CurrencyCode.Create(currency));
    }
}
