using BounceMonitor.Consumers;
using BounceMonitor.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BounceMonitor.Tests;

public class BounceNotificationConsumerSpecs
{
    [Test]
    public async Task Should_publish_event_when_bounce_received()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumersFromNamespaceContaining<EmailBounceConsumer>();
                //
            })
            .BuildServiceProvider(true);

        var harness = await provider.StartTestHarness();

        await harness.Bus.Publish(new AmazonSesBounceNotification
        {
            NotificationType = "??",
            Bounce = new AmazonSesBounce
            {
                Timestamp = DateTime.UtcNow,
                BounceType = "Hard",
                BounceSubType = "Medium",
                BouncedRecipients = new List<AmazonSesBouncedRecipient>
                {
                    new()
                    {
                        EmailAddress = "invalid@unknown.org"
                    }
                }
            }
        });

        Assert.That(await harness.Consumed.Any<AmazonSesBounceNotification>(), Is.True);
        Assert.That(await harness.Published.Any<EmailBounceNotificationReceived>(), Is.True);
    }
}