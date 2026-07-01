namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.UpdateOrganizationDepartment;

public sealed record UpdateOrganizationDepartmentCommandResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid DepartmentId,
    string DepartmentName,
    string DisplayName,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    IReadOnlyList<Guid> CategoryIds,
    int OfficialCount,
    int AssignedReportCount);
