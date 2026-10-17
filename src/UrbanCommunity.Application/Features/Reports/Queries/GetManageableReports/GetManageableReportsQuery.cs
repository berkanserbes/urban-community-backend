using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Reports.Queries.GetReports;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;

public sealed record GetManageableReportsQuery(
    Guid RequestingUserId,
    bool IsAdmin,
    Guid? NeighborhoodId = null,
    ReportStatus? Status = null,
    Guid? CategoryId = null,
    Guid? AssignedOrganizationDepartmentId = null,
    string? Search = null,
    ReportPriority? Priority = null,
    ReportAssignmentFilter? Assignment = null)
    : PaginationRequest, IRequest<Result<PagedResult<GetReportsQueryResponse>>>;
