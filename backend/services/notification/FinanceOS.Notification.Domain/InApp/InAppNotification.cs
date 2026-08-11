using FinanceOS.Notification.Domain.Common;

namespace FinanceOS.Notification.Domain.InApp;

public sealed class InAppNotification
{
    private InAppNotification()
    {
    }

    private InAppNotification(InAppNotificationId id, HouseholdId householdId, string type, string title, string body, DateTimeOffset createdAt)
    {
        Id = id;
        HouseholdId = householdId;
        Type = type;
        Title = title;
        Body = body;
        CreatedAt = createdAt;
    }

    public InAppNotificationId Id { get; }

    public HouseholdId HouseholdId { get; }

    public string Type { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ReadAt { get; private set; }

    public static InAppNotification Create(HouseholdId householdId, string type, string title, string body, DateTimeOffset createdAt)
    {
        if (householdId.Value == Guid.Empty)
        {
            throw new InvalidNotificationException("Household is required.");
        }

        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new InvalidNotificationException("Notification content is required.");
        }

        return new InAppNotification(InAppNotificationId.New(), householdId, type.Trim(), title.Trim(), body.Trim(), createdAt);
    }

    public void MarkRead(DateTimeOffset readAt)
    {
        ReadAt ??= readAt;
    }
}

public sealed class InvalidNotificationException(string message) : DomainException(message);
