using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed record GetReportsQuery(
    Guid? NeighborhoodId = null,
    ReportStatus? Status = null,
    Guid? CategoryId = null,
    Guid? AssignedDepartmentId = null,
    Guid? UserId = null) : IRequest<Result<IReadOnlyList<GetReportsQueryResponse>>>;
