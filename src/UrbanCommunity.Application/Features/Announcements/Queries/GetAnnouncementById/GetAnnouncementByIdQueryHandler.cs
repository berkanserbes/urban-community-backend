using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncementById;

public sealed class GetAnnouncementByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementByIdQuery, Result<GetAnnouncementByIdQueryResponse>>
{
    public async Task<Result<GetAnnouncementByIdQueryResponse>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Province)
            .Include(a => a.District)
            .Include(a => a.Neighborhood)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        return Result<GetAnnouncementByIdQueryResponse>.Success(new GetAnnouncementByIdQueryResponse(
            announcement.Id,
            announcement.Title,
            announcement.Content,
            announcement.UserId,
            announcement.User.GetUserFullName(),
            announcement.Scope,
            announcement.ProvinceId,
            announcement.Province?.Name,
            announcement.DistrictId,
            announcement.District?.Name,
            announcement.NeighborhoodId,
            announcement.Neighborhood?.Name,
            announcement.IsActive,
            announcement.ExpiresAt,
            announcement.CreatedAt,
            announcement.UpdatedAt));
    }
}
