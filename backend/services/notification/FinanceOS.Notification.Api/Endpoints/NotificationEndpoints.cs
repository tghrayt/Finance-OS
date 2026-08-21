using FinanceOS.BuildingBlocks.Security;
using FinanceOS.Notification.Application.Common;
using FinanceOS.Notification.Application.Features.InApp.GetNotifications;
using FinanceOS.Notification.Application.Features.InApp.MarkNotificationRead;
using FinanceOS.Notification.Domain.Common;

namespace FinanceOS.Notification.Api.Endpoints;

internal static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(
        this IEndpointRouteBuilder endpoints,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var group = endpoints
            .MapGroup("/api/v1/notification")
            .WithTags("Notification")
            .RequireFinanceOSAuthorization(configuration, environment);

        group.MapGet("/in-app", GetInAppNotificationsAsync).WithName("GetInAppNotifications");
        group.MapPut("/in-app/{notificationId:guid}/read", MarkInAppNotificationReadAsync).WithName("MarkInAppNotificationRead");

        return endpoints;
    }

    private static async Task<IResult> GetInAppNotificationsAsync(Guid householdId, int page, int pageSize, GetNotificationsHandler handler, CancellationToken cancellationToken) =>
        await ExecuteAsync(async () => Results.Ok(await handler.HandleAsync(householdId, page, pageSize, cancellationToken)));

    private static async Task<IResult> MarkInAppNotificationReadAsync(Guid householdId, Guid notificationId, MarkNotificationReadHandler handler, CancellationToken cancellationToken) =>
        await ExecuteAsync(async () => Results.Ok(await handler.HandleAsync(new MarkNotificationReadCommand(householdId, notificationId), cancellationToken)));

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (DomainException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (NotificationNotFoundException exception)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }
}
