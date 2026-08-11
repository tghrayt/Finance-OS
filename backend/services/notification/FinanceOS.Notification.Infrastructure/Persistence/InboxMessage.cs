namespace FinanceOS.Notification.Infrastructure.Persistence;

public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    public InboxMessage(Guid messageId, string consumerName, DateTimeOffset processedAt)
    {
        MessageId = messageId;
        ConsumerName = consumerName;
        ProcessedAt = processedAt;
    }

    public Guid MessageId { get; private set; }

    public string ConsumerName { get; private set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; private set; }
}
