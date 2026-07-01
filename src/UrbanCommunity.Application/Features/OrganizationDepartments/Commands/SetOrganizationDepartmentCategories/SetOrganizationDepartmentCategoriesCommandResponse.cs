namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentCategories;

public sealed record SetOrganizationDepartmentCategoriesCommandResponse(
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
