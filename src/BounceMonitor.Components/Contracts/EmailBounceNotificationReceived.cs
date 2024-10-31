namespace BounceMonitor.Contracts;

public record EmailBounceNotificationReceived
{
    public string EmailAddress { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? BounceType { get; set; }
    public string? BounceSubType { get; set; }
}