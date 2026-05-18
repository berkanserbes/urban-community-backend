using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateAnnouncementCommand, Result<CreateAnnouncementCommandResponse>>
{
    public async Task<Result<CreateAnnouncementCommandResponse>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (request.NeighborhoodId.HasValue)
        {
            var neighborhoodExists = await context.Neighborhoods
                .AsNoTracking()
                .AnyAsync(n => n.Id == request.NeighborhoodId.Value, cancellationToken);

            if (!neighborhoodExists)
                throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId.Value);
        }

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var announcement = new Announcement
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            UserId = request.UserId,
            NeighborhoodId = request.NeighborhoodId,
            ExpiresAt = request.ExpiresAt
        };

        context.Announcements.Add(announcement);
        await context.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new AnnouncementPublishedEvent
        {
            AnnouncementId = announcement.Id,
            Title = announcement.Title,
            Content = announcement.Content,
            PublishedByUserId = announcement.UserId,
            PublishedByUserFullName = $"{user.FirstName} {user.LastName}",
            NeighborhoodId = announcement.NeighborhoodId,
            ExpiresAt = announcement.ExpiresAt,
            CreatedAt = announcement.CreatedAt
        }, cancellationToken);

        return Result<CreateAnnouncementCommandResponse>.Success(
            new CreateAnnouncementCommandResponse(
                announcement.Id,
                announcement.Title,
                announcement.IsActive,
                announcement.ExpiresAt,
                announcement.CreatedAt));
    }
}
