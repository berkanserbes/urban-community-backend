using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAnnouncement;

public sealed record ActivateAnnouncementCommand(
    Guid Id,
    Guid RequestingUserId,
    DateTime? ExpiresAt = null) : IRequest<Result<ActivateAnnouncementCommandResponse>>;
