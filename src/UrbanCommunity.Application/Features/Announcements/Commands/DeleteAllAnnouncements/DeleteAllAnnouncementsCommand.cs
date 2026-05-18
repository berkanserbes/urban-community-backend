using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAllAnnouncements;

public sealed record DeleteAllAnnouncementsCommand : IRequest<Result<DeleteAllAnnouncementsCommandResponse>>;
