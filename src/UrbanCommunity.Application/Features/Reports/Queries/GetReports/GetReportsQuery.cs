using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed record GetReportsQuery(
    Guid? RequestingUserId = null,
    bool IsOfficial = false,
    string? Scope = null,
    ReportStatus? Status = null,
    Guid? CategoryId = null,
    Guid? AssignedOrganizationDepartmentId = null,
    Guid? UserId = null) : IRequest<Result<IReadOnlyList<GetReportsQueryResponse>>>;
