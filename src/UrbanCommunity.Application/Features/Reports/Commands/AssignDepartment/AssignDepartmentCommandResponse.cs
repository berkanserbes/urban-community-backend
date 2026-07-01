using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.AssignDepartment;

public sealed record AssignDepartmentCommandResponse(
    Guid ReportId,
    Guid? OrganizationDepartmentId,
    Guid? OrganizationId,
    string? OrganizationName,
    Guid? DepartmentId,
    string? DepartmentName,
    ReportStatus Status,
    DateTime ChangedAt,
    string Message);
