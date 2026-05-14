namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.AddReportComment;

public sealed record AddReportCommentCommandResponse(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt);
