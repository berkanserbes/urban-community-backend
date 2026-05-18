using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAllAnnouncements;

public sealed record ActivateAllAnnouncementsCommand(DateTime? ExpiresAt = null)
    : IRequest<Result<ActivateAllAnnouncementsCommandResponse>>;
