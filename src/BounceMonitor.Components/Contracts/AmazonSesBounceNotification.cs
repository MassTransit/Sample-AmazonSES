namespace BounceMonitor.Contracts;

public record AmazonSesBounceNotification
{
    public string? NotificationType { get; set; }
    public AmazonSesBounce? Bounce { get; set; }
}