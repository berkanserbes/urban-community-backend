using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class ReportHistory : BaseEntity
{
    public Guid ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public ReportStatus FromStatus { get; set; }
    public ReportStatus ToStatus { get; set; }
    
    public string? Note { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }
}
