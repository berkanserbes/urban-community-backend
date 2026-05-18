using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    string Title,
    string Content,
    Guid? NeighborhoodId,
    DateTime? ExpiresAt,
    Guid UserId) : IRequest<Result<CreateAnnouncementCommandResponse>>;
