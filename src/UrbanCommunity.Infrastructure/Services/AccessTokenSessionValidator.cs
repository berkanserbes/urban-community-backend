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
                user.OrganizationDepartmentId,
                user.MustChangePassword,
                user.OfficialJurisdictionLevel,
                OfficialJurisdictionId =
                    user.OfficialProvinceId ??
                    user.OfficialDistrictId ??
                    user.OfficialNeighborhoodId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null ||
            !user.IsActive ||
            user.Role != claims.Role ||
            user.MustChangePassword != claims.MustChangePassword)
        {
            return false;
        }

        if (user.Role != UserRole.Official)
        {
            return !claims.OrganizationDepartmentId.HasValue &&
                   !claims.OfficialJurisdictionLevel.HasValue &&
                   !claims.OfficialJurisdictionId.HasValue;
        }

        return user.OrganizationDepartmentId == claims.OrganizationDepartmentId &&
               user.OfficialJurisdictionLevel == claims.OfficialJurisdictionLevel &&
               user.OfficialJurisdictionId == claims.OfficialJurisdictionId;
    }
}
