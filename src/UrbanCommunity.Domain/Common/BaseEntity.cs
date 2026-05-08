namespace UrbanCommunity.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public void SetCreatedAt(DateTime now) => CreatedAt = now;
    public void SetUpdatedAt(DateTime now) => UpdatedAt = now;
}
