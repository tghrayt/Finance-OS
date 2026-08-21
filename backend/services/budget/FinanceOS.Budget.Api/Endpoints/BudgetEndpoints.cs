using FinanceOS.BuildingBlocks.Security;
using FinanceOS.Budget.Application.Common;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.CreateMonthlyBudget;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.GetMonthlyBudget;
using FinanceOS.Budget.Application.Features.MonthlyBudgets.SetBudgetAllocation;
using FinanceOS.Budget.Domain.Common;

namespace FinanceOS.Budget.Api.Endpoints;

internal static class BudgetEndpoints
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(
        this IEndpointRouteBuilder endpoints,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var group = endpoints
            .MapGroup("/api/v1/budget")
            .WithTags("Budget")
            .RequireFinanceOSAuthorization(configuration, environment);

        group.MapPost("/monthly-budgets", CreateMonthlyBudgetAsync).WithName("CreateMonthlyBudget");
        group.MapGet("/monthly-budgets/current", GetMonthlyBudgetAsync).WithName("GetMonthlyBudget");
        group.MapPut("/monthly-budgets/{budgetId:guid}/allocations/{categoryId:guid}", SetBudgetAllocationAsync).WithName("SetBudgetAllocation");

        return endpoints;
    }

    private static async Task<IResult> CreateMonthlyBudgetAsync(
        CreateMonthlyBudgetRequest request,
        HttpContext httpContext,
        IHouseholdAccessVerifier householdAccess,
        CreateMonthlyBudgetHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            if (!await householdAccess.CanAccessHouseholdAsync(request.HouseholdId, httpContext, cancellationToken))
            {
                return Results.Forbid();
            }

            var result = await handler.HandleAsync(
                new CreateMonthlyBudgetCommand(request.HouseholdId, request.Year, request.Month, request.TotalBudget, request.Currency),
                cancellationToken);

            return Results.Created($"/api/v1/budget/monthly-budgets/{result.BudgetId}", result);
        });
    }

    private static async Task<IResult> GetMonthlyBudgetAsync(
        Guid householdId,
        int year,
        int month,
        HttpContext httpContext,
        IHouseholdAccessVerifier householdAccess,
        GetMonthlyBudgetHandler handler,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(async () =>
            await householdAccess.CanAccessHouseholdAsync(householdId, httpContext, cancellationToken)
                ? Results.Ok(await handler.HandleAsync(householdId, year, month, cancellationToken))
                : Results.Forbid());

    private static async Task<IResult> SetBudgetAllocationAsync(
        Guid budgetId,
        Guid categoryId,
        Guid householdId,
        SetBudgetAllocationRequest request,
        HttpContext httpContext,
        IHouseholdAccessVerifier householdAccess,
        SetBudgetAllocationHandler handler,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
            await householdAccess.CanAccessHouseholdAsync(householdId, httpContext, cancellationToken)
                ? Results.Ok(await handler.HandleAsync(new SetBudgetAllocationCommand(householdId, budgetId, categoryId, request.PlannedAmount), cancellationToken))
                : Results.Forbid());
    }

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (BudgetValidationException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (DomainException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (BudgetNotFoundException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (BudgetConflictException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }
}

internal sealed record CreateMonthlyBudgetRequest(Guid HouseholdId, int Year, int Month, decimal TotalBudget, string Currency);

internal sealed record SetBudgetAllocationRequest(decimal PlannedAmount);
