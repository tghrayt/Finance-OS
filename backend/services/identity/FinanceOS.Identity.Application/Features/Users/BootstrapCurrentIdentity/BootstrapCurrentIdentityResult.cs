using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Users.GetUser;

namespace FinanceOS.Identity.Application.Features.Users.BootstrapCurrentIdentity;

public sealed record BootstrapCurrentIdentityResult(
    UserDetailsResult User,
    HouseholdDetailsResult Household);
