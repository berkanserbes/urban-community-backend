namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.UpdateReportComment;

public sealed record UpdateReportCommentCommandResponse(
    Guid Id,
    string Content,
    DateTime? UpdatedAt);
