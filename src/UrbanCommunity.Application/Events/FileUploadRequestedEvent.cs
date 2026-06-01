namespace UrbanCommunity.Application.Events;

public sealed record FileUploadRequestedEvent
{
    public Guid CorrelationId { get; init; } = Guid.CreateVersion7();
    public required Guid UserId { get; init; }
    public required string TempFilePath { get; init; }
    public required string OriginalFileName { get; init; }
    public required string ContentType { get; init; }
    public required string FolderName { get; init; }
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}
