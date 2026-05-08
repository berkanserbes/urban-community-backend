using System.ComponentModel.DataAnnotations.Schema;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public Guid? RelatedEntityId { get; set; } // ReportId veya SuggestionId olabilir
    public NotificationRelatedEntityType? RelatedEntityType { get; set; }

    [NotMapped]
    public bool HasRelatedEntity => RelatedEntityId.HasValue && RelatedEntityType.HasValue;

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
    }

    public void MarkAsUnread()
    {
        if (!IsRead)
        {
            return;
        }

        IsRead = false;
    }
}
