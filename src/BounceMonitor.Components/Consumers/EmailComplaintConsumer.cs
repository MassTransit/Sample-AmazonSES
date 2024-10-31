using BounceMonitor.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BounceMonitor.Consumers;

public class EmailComplaintConsumer :
    IConsumer<AmazonSesComplaintNotification>
{
    readonly ILogger<EmailComplaintConsumer> _logger;

    public EmailComplaintConsumer(ILogger<EmailComplaintConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AmazonSesComplaintNotification> context)
    {
        if (context.Message.Complaint is null)
        {
            _logger.LogDebug("Invalid complaint notification received: {NotificationType}", context.Message.NotificationType);
            return;
        }

        var complaint = context.Message.Complaint;

        var recipients = complaint.ComplainedRecipients?.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).ToList();
        if (recipients == null)
        {
            _logger.LogDebug("Empty complaint recipient list received: {NotificationType}", context.Message.NotificationType);
            return;
        }

        foreach (var recipient in recipients)
        {
            _logger.LogInformation("Complaint: {EmailAddress} ({MessageId})", recipient.EmailAddress, complaint.MessageId);

            await context.Publish(new EmailComplaintNotificationReceived
            {
                EmailAddress = recipient.EmailAddress!,
                Timestamp = complaint.Timestamp,
                MessageId = complaint.MessageId
            });
        }
    }
}

public class EmailComplaintConsumerDefinition :
    ConsumerDefinition<EmailComplaintConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<EmailComplaintConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseRawJsonDeserializer(RawSerializerOptions.AnyMessageType, true);
    }
}