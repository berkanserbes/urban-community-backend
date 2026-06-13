using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAllAnnouncements;

public sealed record DeleteAllAnnouncementsCommand(Guid RequestingUserId)
    : IRequest<Result<DeleteAllAnnouncementsCommandResponse>>;
