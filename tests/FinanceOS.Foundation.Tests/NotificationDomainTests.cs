using FinanceOS.Notification.Domain.Common;
using FinanceOS.Notification.Domain.InApp;

namespace FinanceOS.Foundation.Tests;

public sealed class NotificationDomainTests
{
    [Fact]
    public void InAppNotificationRequiresContent()
    {
        var householdId = new HouseholdId(Guid.NewGuid());

        Assert.ThrowsAny<DomainException>(() =>
            InAppNotification.Create(householdId, "budget.threshold", "", "body", DateTimeOffset.UtcNow));

        Assert.ThrowsAny<DomainException>(() =>
            InAppNotification.Create(householdId, "budget.threshold", "title", "", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarkReadIsIdempotent()
    {
        var notification = InAppNotification.Create(
            new HouseholdId(Guid.NewGuid()),
            "budget.threshold",
            "Budget atteint",
            "Une categorie a franchi un seuil.",
            DateTimeOffset.UtcNow);
        var readAt = DateTimeOffset.UtcNow.AddMinutes(2);

        notification.MarkRead(readAt);
        notification.MarkRead(readAt.AddMinutes(5));

        Assert.Equal(readAt, notification.ReadAt);
    }
}
