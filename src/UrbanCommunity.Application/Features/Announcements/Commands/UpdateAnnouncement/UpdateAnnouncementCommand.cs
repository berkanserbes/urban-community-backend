using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.UpdateAnnouncement;

public sealed record UpdateAnnouncementCommand(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Content,
    DateTime? ExpiresAt) : IRequest<Result<UpdateAnnouncementCommandResponse>>;
