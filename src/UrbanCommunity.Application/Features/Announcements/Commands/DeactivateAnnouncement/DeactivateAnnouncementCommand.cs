using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAnnouncement;

public sealed record DeactivateAnnouncementCommand(Guid Id) : IRequest<Result<DeactivateAnnouncementCommandResponse>>;
