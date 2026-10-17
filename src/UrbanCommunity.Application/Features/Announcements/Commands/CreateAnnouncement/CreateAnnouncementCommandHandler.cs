using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler(
    IApplicationDbContext context,
    IAnnouncementAuthorizationService authorizationService,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateAnnouncementCommand, Result<CreateAnnouncementCommandResponse>>
{
    public async Task<Result<CreateAnnouncementCommandResponse>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {

        if (request.ProvinceId.HasValue)
        {
            var provinceExists = await context.Provinces
                .AsNoTracking()
                .AnyAsync(p => p.Id == request.ProvinceId.Value, cancellationToken);
            if (!provinceExists)
                throw new NotFoundException(nameof(Province), request.ProvinceId.Value);
        }

        if (request.DistrictId.HasValue)
        {
            var districtExists = await context.Districts
                .AsNoTracking()
                .AnyAsync(d => d.Id == request.DistrictId.Value, cancellationToken);
            if (!districtExists)
                throw new NotFoundException(nameof(District), request.DistrictId.Value);
        }

        if (request.NeighborhoodId.HasValue)
        {
            var neighborhoodExists = await context.Neighborhoods
                .AsNoTracking()
                .AnyAsync(n => n.Id == request.NeighborhoodId.Value, cancellationToken);
            if (!neighborhoodExists)
                throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId.Value);
        }

        await authorizationService.EnsureCanCreateAsync(
            request.UserId,
            request.Scope,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId,
            cancellationToken);

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var provinceId = request.Scope == AnnouncementScope.Province ? request.ProvinceId : null;
        var districtId = request.Scope == AnnouncementScope.District ? request.DistrictId : null;
        var neighborhoodId = request.Scope == AnnouncementScope.Neighborhood ? request.NeighborhoodId : null;

        var announcement = new Announcement
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            UserId = request.UserId,
            Scope = request.Scope,
            ProvinceId = provinceId,
            DistrictId = districtId,
            NeighborhoodId = neighborhoodId,
            ExpiresAt = request.ExpiresAt
        };

        context.Announcements.Add(announcement);

        // Published through the transactional outbox: SaveChanges stores it with the data below.
        await publishEndpoint.Publish(new AnnouncementPublishedEvent
        {
            AnnouncementId = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            PublishedByUserId = announcement.UserId,
            PublishedByUserFullName = user.GetUserFullName(),
            Scope = announcement.Scope,
            ProvinceId = announcement.ProvinceId,
            DistrictId = announcement.DistrictId,
            NeighborhoodId = announcement.NeighborhoodId,
            ExpiresAt = announcement.ExpiresAt,
            CreatedAt = announcement.CreatedAt
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result<CreateAnnouncementCommandResponse>.Success(
            new CreateAnnouncementCommandResponse(
                announcement.Id,
                announcement.Title,
                announcement.Content,
                announcement.Scope,
                announcement.IsActive,
                announcement.ExpiresAt,
                announcement.CreatedAt));
    }
}
