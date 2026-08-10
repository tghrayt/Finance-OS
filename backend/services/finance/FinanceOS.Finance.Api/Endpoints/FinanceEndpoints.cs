using FinanceOS.Finance.Application.Common;
using FinanceOS.Finance.Application.Features.Accounts.CreateAccount;
using FinanceOS.Finance.Application.Features.Accounts.GetAccounts;
using FinanceOS.Finance.Application.Features.Categories.CreateCategory;
using FinanceOS.Finance.Application.Features.Categories.GetCategories;
using FinanceOS.Finance.Application.Features.Transactions.CreateTransaction;
using FinanceOS.Finance.Application.Features.Transactions.GetTransactions;
using FinanceOS.Finance.Domain.Common;

namespace FinanceOS.Finance.Api.Endpoints;

internal static class FinanceEndpoints
{
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/finance").WithTags("Finance");

        group.MapPost("/accounts", CreateAccountAsync).WithName("CreateAccount");
        group.MapGet("/accounts", GetAccountsAsync).WithName("GetAccounts");
        group.MapPost("/categories", CreateCategoryAsync).WithName("CreateCategory");
        group.MapGet("/categories", GetCategoriesAsync).WithName("GetCategories");
        group.MapPost("/transactions", CreateTransactionAsync).WithName("CreateTransaction");
        group.MapGet("/transactions", GetTransactionsAsync).WithName("GetTransactions");

        return endpoints;
    }

    private static async Task<IResult> CreateAccountAsync(CreateAccountRequest request, CreateAccountHandler handler, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new CreateAccountCommand(request.HouseholdId, request.Name, request.Type, request.InitialBalance, request.Currency, request.InstitutionName),
                cancellationToken);

            return Results.Created($"/api/v1/finance/accounts/{result.AccountId}", result);
        });
    }

    private static async Task<IResult> GetAccountsAsync(Guid householdId, GetAccountsHandler handler, CancellationToken cancellationToken) =>
        await ExecuteAsync(async () => Results.Ok(await handler.HandleAsync(householdId, cancellationToken)));

    private static async Task<IResult> CreateCategoryAsync(CreateCategoryRequest request, CreateCategoryHandler handler, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(new CreateCategoryCommand(request.HouseholdId, request.Name, request.ParentCategoryId, request.Icon), cancellationToken);
            return Results.Created($"/api/v1/finance/categories/{result.CategoryId}", result);
        });
    }

    private static async Task<IResult> GetCategoriesAsync(Guid householdId, GetCategoriesHandler handler, CancellationToken cancellationToken) =>
        await ExecuteAsync(async () => Results.Ok(await handler.HandleAsync(householdId, cancellationToken)));

    private static async Task<IResult> CreateTransactionAsync(CreateTransactionRequest request, HttpContext httpContext, CreateTransactionHandler handler, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await handler.HandleAsync(
                new CreateTransactionCommand(
                    request.HouseholdId,
                    request.AccountId,
                    request.DestinationAccountId,
                    request.Type,
                    request.Amount,
                    request.Currency,
                    request.CategoryId,
                    request.Merchant,
                    request.Description,
                    request.TransactionDate,
                    ResolveCorrelationId(httpContext, request.CorrelationId)),
                cancellationToken);

            return Results.Created($"/api/v1/finance/transactions/{result.TransactionId}", result);
        });
    }

    private static async Task<IResult> GetTransactionsAsync(Guid householdId, int page, int pageSize, GetTransactionsHandler handler, CancellationToken cancellationToken) =>
        await ExecuteAsync(async () => Results.Ok(await handler.HandleAsync(householdId, page, pageSize, cancellationToken)));

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (FinanceValidationException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (DomainException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (FinanceNotFoundException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }

    private static Guid ResolveCorrelationId(HttpContext httpContext, Guid? requestCorrelationId)
    {
        if (requestCorrelationId is { } value && value != Guid.Empty)
        {
            return value;
        }

        return Guid.TryParse(httpContext.TraceIdentifier, out var traceGuid) ? traceGuid : Guid.NewGuid();
    }
}

internal sealed record CreateAccountRequest(Guid HouseholdId, string Name, string Type, decimal InitialBalance, string Currency, string? InstitutionName);

internal sealed record CreateCategoryRequest(Guid HouseholdId, string Name, Guid? ParentCategoryId, string? Icon);

internal sealed record CreateTransactionRequest(
    Guid HouseholdId,
    Guid AccountId,
    Guid? DestinationAccountId,
    string Type,
    decimal Amount,
    string Currency,
    Guid? CategoryId,
    string? Merchant,
    string? Description,
    DateOnly TransactionDate,
    Guid? CorrelationId);
