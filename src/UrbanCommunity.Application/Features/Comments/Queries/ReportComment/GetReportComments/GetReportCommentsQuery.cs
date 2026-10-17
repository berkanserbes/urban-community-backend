using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;

public sealed record GetReportCommentsQuery(Guid ReportId) : PaginationRequest, IRequest<Result<PagedResult<GetReportCommentsQueryResponse>>>;
