namespace BounceMonitor.Contracts;

public record EmailComplaintNotificationReceived
{
    public string EmailAddress { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? MessageId { get; set; }
}