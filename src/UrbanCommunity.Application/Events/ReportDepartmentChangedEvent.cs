namespace UrbanCommunity.Application.Events;

public sealed record ReportDepartmentChangedEvent
{
    public Guid ReportId { get; init; }
    public string ReportTitle { get; init; } = string.Empty;
    public Guid ReportOwnerId { get; init; }
    public Guid ProvinceId { get; init; }
    public Guid DistrictId { get; init; }
    public Guid NeighborhoodId { get; init; }
    public Guid? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public DateTime ChangedAt { get; init; }
}
