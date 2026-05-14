using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserQueryResponse>>
{
    public async Task<Result<GetCurrentUserQueryResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new GetCurrentUserQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Email!,
                u.PhoneNumber,
                u.Role,
                u.IsActive,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null,
                u.Bio,
                u.ProfileImageUrl,
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        return Result<GetCurrentUserQueryResponse>.Success(user);
    }
}
