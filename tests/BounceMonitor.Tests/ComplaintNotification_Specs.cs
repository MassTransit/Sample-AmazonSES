using BounceMonitor.Consumers;
using BounceMonitor.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BounceMonitor.Tests;

public class ComplaintNotificationConsumerSpecs
{
    [Test]
    public async Task Should_publish_event_when_bounce_received()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumersFromNamespaceContaining<EmailComplaintConsumer>();
                x.SetKebabCaseEndpointNameFormatter();
                //
            })
            .BuildServiceProvider(true);

        var harness = await provider.StartTestHarness();

        await harness.Bus.Publish(new AmazonSesComplaintNotification
        {
            NotificationType = "??",
            Complaint = new AmazonSesComplaint
            {
                Timestamp = DateTime.UtcNow,
                MessageId = NewId.NextGuid().ToString(),
                ComplainedRecipients = new List<AmazonSesComplainedRecipient>
                {
                    new()
                    {
                        EmailAddress = "invalid@unknown.org"
                    }
                }
            }
        });

        Assert.That(await harness.Consumed.Any<AmazonSesComplaintNotification>(), Is.True);
        Assert.That(await harness.Published.Any<EmailComplaintNotificationReceived>(), Is.True);
    }
}