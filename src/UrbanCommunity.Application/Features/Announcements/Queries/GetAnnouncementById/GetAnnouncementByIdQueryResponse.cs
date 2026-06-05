using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncementById;

public sealed record GetAnnouncementByIdQueryResponse(
    Guid Id,
    string Title,
    string Content,
    Guid UserId,
    string UserFullName,
    AnnouncementScope Scope,
    Guid? ProvinceId,
    string? ProvinceName,
    Guid? DistrictId,
    string? DistrictName,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
