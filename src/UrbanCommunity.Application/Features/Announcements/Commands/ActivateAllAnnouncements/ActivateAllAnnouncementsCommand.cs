using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAllAnnouncements;

public sealed record ActivateAllAnnouncementsCommand(
    Guid RequestingUserId,
    DateTime? ExpiresAt = null)
    : IRequest<Result<ActivateAllAnnouncementsCommandResponse>>;
