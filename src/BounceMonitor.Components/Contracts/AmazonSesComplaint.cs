namespace BounceMonitor.Contracts;

public record AmazonSesComplaint
{
    public string? MessageId { get; set; }
    public DateTime Timestamp { get; set; }
    public List<AmazonSesComplainedRecipient>? ComplainedRecipients { get; set; }
}