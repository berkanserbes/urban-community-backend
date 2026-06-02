using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    string Title,
    string Content,
    AnnouncementScope Scope,
    Guid? ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId,
    DateTime? ExpiresAt,
    Guid UserId) : IRequest<Result<CreateAnnouncementCommandResponse>>;
