using MassTransit;
using UrbanCommunity.Infrastructure.Consumers;

namespace UrbanCommunity.Api.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Registers the consumers. The caller picks the transport, so Program and tests can share
    /// the same registration.
    /// </summary>
    public static IBusRegistrationConfigurator AddMessaging(this IBusRegistrationConfigurator bus)
    {
        bus.SetKebabCaseEndpointNameFormatter();

        bus.AddConsumer<
            ReportAiReviewRequestedConsumer,
            ReportAiReviewRequestedConsumerDefinition>();
        bus.AddConsumer<ReportAiReviewFaultConsumer>();
        bus.AddConsumer<ReportCreatedConsumer>();
        bus.AddConsumer<ReportStatusChangedConsumer>();
        bus.AddConsumer<ReportDepartmentChangedConsumer>();
        bus.AddConsumer<FileUploadConsumer>();
        bus.AddConsumer<AnnouncementPublishedConsumer>();
        bus.AddConsumer<SuggestionStatusChangedConsumer>();
        bus.AddConsumer<SuggestionOfficialResponseUpdatedConsumer>();

        return bus;
    }
}
