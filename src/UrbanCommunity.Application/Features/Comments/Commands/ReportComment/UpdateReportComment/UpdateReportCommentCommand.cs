using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.UpdateReportComment;

public sealed record UpdateReportCommentCommand(
    Guid CommentId,
    Guid RequestingUserId,
    string Content) : IRequest<Result<UpdateReportCommentCommandResponse>>;
