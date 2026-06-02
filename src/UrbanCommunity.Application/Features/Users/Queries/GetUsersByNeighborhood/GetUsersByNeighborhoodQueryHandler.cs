using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;

public sealed class GetUsersByNeighborhoodQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUsersByNeighborhoodQuery, Result<IReadOnlyList<GetUsersByNeighborhoodQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetUsersByNeighborhoodQueryResponse>>> Handle(
        GetUsersByNeighborhoodQuery request,
        CancellationToken cancellationToken)
    {
        var neighborhoodExists = await context.Neighborhoods
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.NeighborhoodId, cancellationToken);

        if (!neighborhoodExists)
            throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId);

        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.NeighborhoodId == request.NeighborhoodId && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new GetUsersByNeighborhoodQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role,
                u.Bio,
                u.ProfileImageUrl,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetUsersByNeighborhoodQueryResponse>>.Success(users);
    }
}
