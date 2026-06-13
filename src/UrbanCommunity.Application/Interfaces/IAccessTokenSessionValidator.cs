using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Interfaces;

public interface IAccessTokenSessionValidator
{
    Task<bool> IsValidAsync(
        AccessTokenSessionClaims claims,
        CancellationToken cancellationToken);
}

public sealed record AccessTokenSessionClaims(
    Guid UserId,
    UserRole Role,
    Guid? DepartmentId,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId);
