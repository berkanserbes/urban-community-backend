namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;

public sealed record GetReportCommentsQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
