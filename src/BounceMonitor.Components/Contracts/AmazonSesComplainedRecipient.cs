namespace BounceMonitor.Contracts;

public record AmazonSesComplainedRecipient
{
    public string? EmailAddress { get; set; }
}