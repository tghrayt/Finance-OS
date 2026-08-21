using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.AddHouseholdMember;
using FinanceOS.Identity.Application.Features.Households.ChangeHouseholdMemberRole;
using FinanceOS.Identity.Application.Features.Households.CreateHousehold;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Households.GetHousehold;
using FinanceOS.Identity.Application.Features.Households.RemoveHouseholdMember;
using FinanceOS.Identity.Application.Features.Users.BootstrapCurrentIdentity;
using FinanceOS.Identity.Application.Features.Users.CreateUser;
using FinanceOS.Identity.Application.Features.Users.GetUser;
using FinanceOS.Identity.Application.Features.Users.UpdateUserProfile;
using FinanceOS.Identity.Api.Security;
using FinanceOS.Identity.Domain.Common;
using System.Security.Claims;

namespace FinanceOS.Identity.Api.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder endpoints,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var group = endpoints
            .MapGroup("/api/v1/identity")
            .WithTags("Identity");

        group.MapPost("/users", CreateUserAsync)
            .WithName("CreateIdentityUser")
            .AllowAnonymous();

        var protectedGroup = group.MapGroup("");
        if (configuration.ShouldRequireIdentityAuthorization(environment))
        {
            protectedGroup.RequireAuthorization(IdentityPolicies.AuthenticatedUser);
        }

        protectedGroup.MapGet("/users/{userId:guid}", GetUserAsync)
            .WithName("GetIdentityUser");

        protectedGroup.MapGet("/users/me", GetMeAsync)
            .WithName("GetCurrentIdentityUser");

        protectedGroup.MapPost("/me/bootstrap", BootstrapCurrentIdentityAsync)
            .WithName("BootstrapCurrentIdentity");

        protectedGroup.MapPut("/users/{userId:guid}/profile", UpdateUserProfileAsync)
            .WithName("UpdateIdentityUserProfile");

        protectedGroup.MapPost("/households", CreateHouseholdAsync)
            .WithName("CreateHousehold");

        protectedGroup.MapGet("/households/{householdId:guid}", GetHouseholdAsync)
            .WithName("GetHousehold");

        protectedGroup.MapGet("/households/current", GetCurrentHouseholdAsync)
            .WithName("GetCurrentHousehold");

        var householdManagementGroup = protectedGroup
            .MapGroup("/households/{householdId:guid}/members");

        if (configuration.ShouldRequireIdentityAuthorization(environment))
        {
            householdManagementGroup.RequireAuthorization(IdentityPolicies.CanManageHousehold);
        }

        householdManagementGroup.MapPost("", AddHouseholdMemberAsync)
            .WithName("AddHouseholdMember");

        householdManagementGroup.MapPut("/{userId:guid}/role", ChangeHouseholdMemberRoleAsync)
            .WithName("ChangeHouseholdMemberRole");

        householdManagementGroup.MapDelete("/{userId:guid}", RemoveHouseholdMemberAsync)
            .WithName("RemoveHouseholdMember");

        return endpoints;
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        CreateUserHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new CreateUserCommand(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.PreferredCurrency,
                    request.Language,
                    request.TimeZone),
                cancellationToken);

            return Results.Created($"/api/v1/identity/users/{result.UserId}", result);
        });
    }

    private static async Task<IResult> GetUserAsync(
        Guid userId,
        GetUserHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(userId, cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal principal,
        BootstrapCurrentIdentityHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(CreateBootstrapCommand(principal), cancellationToken);

            return Results.Ok(result.User);
        });
    }

    private static async Task<IResult> UpdateUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        UpdateUserProfileHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new UpdateUserProfileCommand(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.PreferredCurrency,
                    request.Language,
                    request.TimeZone),
                cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> CreateHouseholdAsync(
        CreateHouseholdRequest request,
        CreateHouseholdHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new CreateHouseholdCommand(request.OwnerUserId, request.Name, request.Currency),
                cancellationToken);

            return Results.Created($"/api/v1/identity/households/{result.HouseholdId}", result);
        });
    }

    private static async Task<IResult> GetHouseholdAsync(
        Guid householdId,
        GetHouseholdHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(householdId, cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> GetCurrentHouseholdAsync(
        ClaimsPrincipal principal,
        BootstrapCurrentIdentityHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(CreateBootstrapCommand(principal), cancellationToken);

            return Results.Ok(result.Household);
        });
    }

    private static async Task<IResult> BootstrapCurrentIdentityAsync(
        ClaimsPrincipal principal,
        BootstrapCurrentIdentityHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(CreateBootstrapCommand(principal), cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> AddHouseholdMemberAsync(
        Guid householdId,
        Guid? actorUserId,
        ClaimsPrincipal principal,
        AddHouseholdMemberRequest request,
        AddHouseholdMemberHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new AddHouseholdMemberCommand(
                    householdId,
                    ResolveActorUserId(principal, actorUserId),
                    request.UserId,
                    request.Role),
                cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> ChangeHouseholdMemberRoleAsync(
        Guid householdId,
        Guid userId,
        Guid? actorUserId,
        ClaimsPrincipal principal,
        ChangeHouseholdMemberRoleRequest request,
        ChangeHouseholdMemberRoleHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new ChangeHouseholdMemberRoleCommand(
                    householdId,
                    ResolveActorUserId(principal, actorUserId),
                    userId,
                    request.Role),
                cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> RemoveHouseholdMemberAsync(
        Guid householdId,
        Guid userId,
        Guid? actorUserId,
        ClaimsPrincipal principal,
        RemoveHouseholdMemberHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new RemoveHouseholdMemberCommand(
                    householdId,
                    ResolveActorUserId(principal, actorUserId),
                    userId),
                cancellationToken);

            return Results.Ok(result);
        });
    }

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (IdentityValidationException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (DomainException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (IdentityNotFoundException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (IdentityForbiddenException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (IdentityConflictException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    private static Guid GetRequiredUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("user_id");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new IdentityValidationException("Authenticated user id claim is missing or invalid.");
        }

        return userId;
    }

    private static BootstrapCurrentIdentityCommand CreateBootstrapCommand(ClaimsPrincipal principal)
    {
        return new BootstrapCurrentIdentityCommand(
            GetRequiredExternalSubject(principal),
            GetRequiredEmail(principal),
            GetDisplayName(principal),
            "EUR",
            "fr",
            "Europe/Paris");
    }

    private static string GetRequiredExternalSubject(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("oid")
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new IdentityValidationException("Authenticated external subject claim is missing.");
        }

        return value;
    }

    private static string GetRequiredEmail(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("emails")
            ?? principal.FindFirstValue("email")
            ?? principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue("upn")
            ?? principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new IdentityValidationException("Authenticated email claim is missing.");
        }

        return value;
    }

    private static string GetDisplayName(ClaimsPrincipal principal)
    {
        var displayName = principal.FindFirstValue("name");
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var givenName = principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.FindFirstValue("given_name");
        var familyName = principal.FindFirstValue(ClaimTypes.Surname) ?? principal.FindFirstValue("family_name");
        var fullName = $"{givenName} {familyName}".Trim();

        return string.IsNullOrWhiteSpace(fullName) ? GetRequiredEmail(principal) : fullName;
    }

    private static Guid ResolveActorUserId(ClaimsPrincipal principal, Guid? fallbackUserId)
    {
        if (principal.Identity?.IsAuthenticated == true)
        {
            return GetRequiredUserId(principal);
        }

        if (fallbackUserId is { } userId && userId != Guid.Empty)
        {
            return userId;
        }

        throw new IdentityValidationException("Actor user id is required until JWT authentication is enabled.");
    }
}

internal sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string PreferredCurrency,
    string Language,
    string TimeZone);

internal sealed record CreateHouseholdRequest(
    Guid OwnerUserId,
    string Name,
    string Currency);

internal sealed record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    string PreferredCurrency,
    string Language,
    string TimeZone);

internal sealed record AddHouseholdMemberRequest(
    Guid UserId,
    string Role);

internal sealed record ChangeHouseholdMemberRoleRequest(string Role);
