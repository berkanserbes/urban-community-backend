using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class TokenServiceTests
{
    [Fact]
    public void GenerateToken_DoesNotAddOfficialClaims_ForNonOfficialUser()
    {
        var user = CreateUser(UserRole.Citizen);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            CreateTokenService().GenerateToken(user));

        Assert.DoesNotContain(token.Claims, claim => claim.Type == "DepartmentId");
        Assert.DoesNotContain(token.Claims, claim => claim.Type == "OfficialJurisdictionLevel");
        Assert.DoesNotContain(token.Claims, claim => claim.Type == "OfficialJurisdictionId");
    }

    [Fact]
    public void GenerateToken_AddsCompleteAccessClaims_ForOfficialUser()
    {
        var organizationDepartmentId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var user = CreateUser(UserRole.Official);
        user.UpdateAccess(
            UserRole.Official,
            organizationDepartmentId,
            OfficialJurisdictionLevel.District,
            jurisdictionId);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            CreateTokenService().GenerateToken(user));

        Assert.Equal(
            organizationDepartmentId.ToString(),
            token.Claims.Single(claim => claim.Type == "OrganizationDepartmentId").Value);
        Assert.Equal(
            OfficialJurisdictionLevel.District.ToString(),
            token.Claims.Single(claim => claim.Type == "OfficialJurisdictionLevel").Value);
        Assert.Equal(
            jurisdictionId.ToString(),
            token.Claims.Single(claim => claim.Type == "OfficialJurisdictionId").Value);
    }

    private static TokenService CreateTokenService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "test-secret-key-that-is-at-least-32-bytes",
                ["JwtSettings:Issuer"] = "UrbanCommunity.Tests",
                ["JwtSettings:Audience"] = "UrbanCommunity.Tests"
            })
            .Build();

        return new TokenService(configuration);
    }

    private static User CreateUser(UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Token",
            LastName = "User",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            Role = role
        };
    }
}
