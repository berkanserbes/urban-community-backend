using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetAllowedReportStatusTransitions;

public sealed record GetAllowedReportStatusTransitionsQuery(
    Guid ReportId,
    Guid RequestingUserId,
    bool IsAdmin)
    : IRequest<Result<GetAllowedReportStatusTransitionsQueryResponse>>;
