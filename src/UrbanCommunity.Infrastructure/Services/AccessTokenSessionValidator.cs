using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class AccessTokenSessionValidator(IApplicationDbContext context)
    : IAccessTokenSessionValidator
{
    public async Task<bool> IsValidAsync(
        AccessTokenSessionClaims claims,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == claims.UserId)
            .Select(user => new
            {
                user.IsActive,
                user.Role,
                user.DepartmentId,
                user.OfficialJurisdictionLevel,
                OfficialJurisdictionId =
                    user.OfficialProvinceId ??
                    user.OfficialDistrictId ??
                    user.OfficialNeighborhoodId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null ||
            !user.IsActive ||
            user.Role != claims.Role)
        {
            return false;
        }

        if (user.Role != UserRole.Official)
        {
            return !claims.DepartmentId.HasValue &&
                   !claims.OfficialJurisdictionLevel.HasValue &&
                   !claims.OfficialJurisdictionId.HasValue;
        }

        return user.DepartmentId == claims.DepartmentId &&
               user.OfficialJurisdictionLevel == claims.OfficialJurisdictionLevel &&
               user.OfficialJurisdictionId == claims.OfficialJurisdictionId;
    }
}
