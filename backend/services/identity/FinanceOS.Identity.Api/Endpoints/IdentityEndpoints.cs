using FinanceOS.Identity.Application.Common;
using FinanceOS.Identity.Application.Features.Households.CreateHousehold;
using FinanceOS.Identity.Application.Features.Households.GetCurrentHousehold;
using FinanceOS.Identity.Application.Features.Users.CreateUser;
using FinanceOS.Identity.Domain.Common;

namespace FinanceOS.Identity.Api.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/identity")
            .WithTags("Identity");

        group.MapPost("/users", CreateUserAsync)
            .WithName("CreateIdentityUser");

        group.MapPost("/households", CreateHouseholdAsync)
            .WithName("CreateHousehold");

        group.MapGet("/households/current", GetCurrentHouseholdAsync)
            .WithName("GetCurrentHousehold");

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

    private static async Task<IResult> GetCurrentHouseholdAsync(
        Guid userId,
        GetCurrentHouseholdHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(userId, cancellationToken);

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
        catch (IdentityConflictException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
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
