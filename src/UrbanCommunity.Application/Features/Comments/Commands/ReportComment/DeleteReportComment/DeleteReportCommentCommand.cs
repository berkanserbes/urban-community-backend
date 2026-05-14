using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.DeleteReportComment;

public sealed record DeleteReportCommentCommand(Guid CommentId, Guid RequestingUserId, bool IsAdmin = false) : IRequest<Result<DeleteReportCommentCommandResponse>>;
