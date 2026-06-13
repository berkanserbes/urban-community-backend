using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services;

internal static class OfficialJurisdictionQueryExtensions
{
    public static IQueryable<User> WhereJurisdictionMatches(
        this IQueryable<User> users,
        Guid provinceId,
        Guid districtId,
        Guid neighborhoodId)
    {
        return users.Where(user =>
            (user.OfficialJurisdictionLevel == OfficialJurisdictionLevel.Province &&
             user.OfficialProvinceId == provinceId) ||
            (user.OfficialJurisdictionLevel == OfficialJurisdictionLevel.District &&
             user.OfficialDistrictId == districtId) ||
            (user.OfficialJurisdictionLevel == OfficialJurisdictionLevel.Neighborhood &&
             user.OfficialNeighborhoodId == neighborhoodId));
    }
}
