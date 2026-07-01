using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Events;

public sealed record SuggestionStatusChangedEvent
{
    public Guid SuggestionId { get; init; }
    public string SuggestionTitle { get; init; } = string.Empty;
    public Guid SuggestionOwnerId { get; init; }
    public SuggestionStatus OldStatus { get; init; }
    public SuggestionStatus NewStatus { get; init; }
    public DateTime ChangedAt { get; init; }
}
