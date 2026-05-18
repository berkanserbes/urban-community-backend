using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(Guid Id) : IRequest<Result<DeleteAnnouncementCommandResponse>>;
