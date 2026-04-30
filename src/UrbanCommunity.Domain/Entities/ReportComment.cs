using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class ReportComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ReportId { get; set; }
    public Report Report { get; set; } = null!;
}