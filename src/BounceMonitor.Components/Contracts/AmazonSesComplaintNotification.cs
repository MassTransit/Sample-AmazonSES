namespace BounceMonitor.Contracts;

public record AmazonSesComplaintNotification
{
    public string? NotificationType { get; set; }
    public AmazonSesComplaint? Complaint { get; set; }
}