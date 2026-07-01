namespace UrbanCommunity.Application.Events;

public sealed record SuggestionOfficialResponseUpdatedEvent
{
    public Guid SuggestionId { get; init; }
    public string SuggestionTitle { get; init; } = string.Empty;
    public Guid SuggestionOwnerId { get; init; }
    public DateTime UpdatedAt { get; init; }
}
