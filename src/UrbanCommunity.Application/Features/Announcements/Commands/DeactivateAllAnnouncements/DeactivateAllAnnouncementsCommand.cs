using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAllAnnouncements;

public sealed record DeactivateAllAnnouncementsCommand(Guid RequestingUserId)
    : IRequest<Result<DeactivateAllAnnouncementsCommandResponse>>;
