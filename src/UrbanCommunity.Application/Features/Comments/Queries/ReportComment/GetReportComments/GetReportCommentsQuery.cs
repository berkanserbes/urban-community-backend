using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;

public sealed record GetReportCommentsQuery(Guid ReportId) : IRequest<Result<IReadOnlyList<GetReportCommentsQueryResponse>>>;
