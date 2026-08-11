namespace FinanceOS.Budget.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Content = string.Empty;
    }

    public OutboxMessage(Guid id, string type, string content, DateTimeOffset occurredAt)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public string? Error { get; private set; }

    public void MarkProcessed(DateTimeOffset processedAt)
    {
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
    }
}
