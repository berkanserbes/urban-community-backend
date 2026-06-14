using MassTransit;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportAiReviewRequestedConsumerDefinition
    : ConsumerDefinition<ReportAiReviewRequestedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ReportAiReviewRequestedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(retry =>
        {
            retry.Handle<Services.Ai.AiServiceTransientException>();
            retry.Intervals(
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10));
        });
    }
}
