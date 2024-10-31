using BounceMonitor.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BounceMonitor.Consumers;

public class EmailBounceConsumer :
    IConsumer<AmazonSesBounceNotification>
{
    readonly ILogger<EmailBounceConsumer> _logger;

    public EmailBounceConsumer(ILogger<EmailBounceConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AmazonSesBounceNotification> context)
    {
        if (context.Message.Bounce is null)
        {
            _logger.LogDebug("Invalid bounce notification received: {NotificationType}", context.Message.NotificationType);
            return;
        }

        AmazonSesBounce bounce = context.Message.Bounce;

        var recipients = bounce.BouncedRecipients?.Where(x => !string.IsNullOrEmpty(x.EmailAddress)).ToList();
        if (recipients == null)
        {
            _logger.LogDebug("Empty recipient list received: {NotificationType}", context.Message.NotificationType);
            return;
        }

        foreach (var recipient in recipients)
        {
            _logger.LogInformation("Bounced: {EmailAddress} ({BounceType}, {BounceSubType})", recipient.EmailAddress, bounce.BounceType, bounce.BounceSubType);

            await context.Publish(new EmailBounceNotificationReceived
            {
                EmailAddress = recipient.EmailAddress!,
                Timestamp = bounce.Timestamp,
                BounceType = bounce.BounceType,
                BounceSubType = bounce.BounceSubType
            });
        }
    }
}

public class EmailBounceConsumerDefinition :
    ConsumerDefinition<EmailBounceConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<EmailBounceConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseRawJsonDeserializer(RawSerializerOptions.AnyMessageType, true);
    }
}