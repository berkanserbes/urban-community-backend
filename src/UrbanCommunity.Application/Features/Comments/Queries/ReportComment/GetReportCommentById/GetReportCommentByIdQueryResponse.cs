namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportCommentById;

public sealed record GetReportCommentByIdQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
