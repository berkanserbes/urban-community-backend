using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportCommentById;

public sealed record GetReportCommentByIdQuery(Guid CommentId)
    : IRequest<Result<GetReportCommentByIdQueryResponse>>;
