using System.ComponentModel.DataAnnotations.Schema;
using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Publisher
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Target neighborhood (null indicates the whole city)
    public Guid? NeighborhoodId { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }

    [NotMapped]
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    public bool IsVisibleAt(DateTime utcNow)
    {
        return IsActive && (!ExpiresAt.HasValue || ExpiresAt.Value > utcNow);
    }

    public void Activate(DateTime? expiresAt = null)
    {
        IsActive = true;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
