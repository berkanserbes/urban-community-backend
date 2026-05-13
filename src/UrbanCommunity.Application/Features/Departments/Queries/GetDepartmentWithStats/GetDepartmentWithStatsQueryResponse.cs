namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentWithStats;

public sealed record GetDepartmentWithStatsQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    int OfficialCount,
    int AssignedReportCount);
