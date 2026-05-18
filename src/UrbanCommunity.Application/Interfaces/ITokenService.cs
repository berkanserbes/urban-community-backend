using System.Security.Claims;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
