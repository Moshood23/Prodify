namespace Prodify.Infrastructure.Messaging.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    private OutboxMessage()
    {
    }

    public OutboxMessage(string type, string payload)
    {
        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        OccurredAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public bool IsProcessed => ProcessedAt.HasValue;
}