using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.AddReportComment;

public sealed record AddReportCommentCommand(Guid ReportId, Guid UserId, string Content) : IRequest<Result<AddReportCommentCommandResponse>>;