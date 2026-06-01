using System.ComponentModel.DataAnnotations.Schema;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    // Publisher
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Target scope
    public AnnouncementScope Scope { get; set; } = AnnouncementScope.All;

    // Target locations
    public Guid? ProvinceId { get; set; }
    public Province? Province { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

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
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
    }
}
