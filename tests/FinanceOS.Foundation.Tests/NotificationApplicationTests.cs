using FinanceOS.Notification.Application.Abstractions;
using FinanceOS.Notification.Application.Common;
using FinanceOS.Notification.Application.Features.InApp.GetNotifications;
using FinanceOS.Notification.Application.Features.InApp.MarkNotificationRead;
using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;

namespace FinanceOS.Foundation.Tests;

public sealed class NotificationApplicationTests
{
    [Fact]
    public async Task GetNotificationsReturnsNewestFirst()
    {
        var householdId = new HouseholdId(Guid.NewGuid());
        var repository = new InMemoryNotificationRepository();
        await repository.AddAsync(InAppNotification.Create(householdId, "budget.threshold", "Old", "Body", DateTimeOffset.UtcNow.AddDays(-1)));
        await repository.AddAsync(InAppNotification.Create(householdId, "budget.exceeded", "New", "Body", DateTimeOffset.UtcNow));
        var handler = new GetNotificationsHandler(repository);

        var result = await handler.HandleAsync(householdId.Value, page: 1, pageSize: 20, CancellationToken.None);
        var items = result.ToArray();

        Assert.Equal(2, items.Length);
        Assert.Equal("New", items[0].Title);
        Assert.Equal("Old", items[1].Title);
    }

    [Fact]
    public async Task MarkNotificationReadMarksExistingNotification()
    {
        var householdId = new HouseholdId(Guid.NewGuid());
        var repository = new InMemoryNotificationRepository();
        var unitOfWork = new InMemoryNotificationUnitOfWork();
        var notification = InAppNotification.Create(householdId, "budget.threshold", "Budget atteint", "Body", DateTimeOffset.UtcNow);
        await repository.AddAsync(notification);
        var handler = new MarkNotificationReadHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new MarkNotificationReadCommand(householdId.Value, notification.Id.Value), CancellationToken.None);

        Assert.NotNull(result.ReadAt);
        Assert.NotNull(notification.ReadAt);
        Assert.True(unitOfWork.WasSaved);
    }

    [Fact]
    public async Task MarkNotificationReadRejectsUnknownNotification()
    {
        var handler = new MarkNotificationReadHandler(new InMemoryNotificationRepository(), new InMemoryNotificationUnitOfWork());

        await Assert.ThrowsAnyAsync<NotificationApplicationException>(() =>
            handler.HandleAsync(new MarkNotificationReadCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    private sealed class InMemoryNotificationRepository : IInAppNotificationRepository
    {
        private readonly List<InAppNotification> notifications = [];

        public Task AddAsync(InAppNotification notification, CancellationToken cancellationToken = default)
        {
            notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<InAppNotification?> GetByIdAsync(
            InAppNotificationId id,
            HouseholdId householdId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(notifications.SingleOrDefault(notification =>
                notification.Id == id && notification.HouseholdId == householdId));
        }

        public Task<IReadOnlyCollection<InAppNotification>> ListByHouseholdAsync(
            HouseholdId householdId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<InAppNotification> result = notifications
                .Where(notification => notification.HouseholdId == householdId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(result);
        }
    }

    private sealed class InMemoryNotificationUnitOfWork : INotificationUnitOfWork
    {
        public bool WasSaved { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            WasSaved = true;
            return Task.CompletedTask;
        }
    }
}
