using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReportStats;

public sealed record GetManageableReportStatsQuery(Guid RequestingUserId, bool IsAdmin)
    : IRequest<Result<GetManageableReportStatsQueryResponse>>;
