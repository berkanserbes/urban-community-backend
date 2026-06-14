using UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Users;

public sealed class UpdateUserAccessTests
{
    [Theory]
    [InlineData(OfficialJurisdictionLevel.Province)]
    [InlineData(OfficialJurisdictionLevel.District)]
    [InlineData(OfficialJurisdictionLevel.Neighborhood)]
    public void UpdateAccess_OfficialSetsOnlySelectedJurisdiction(
        OfficialJurisdictionLevel level)
    {
        var user = new User();
        var departmentId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();

        user.UpdateAccess(
            UserRole.Official,
            departmentId,
            level,
            jurisdictionId);

        Assert.Equal(UserRole.Official, user.Role);
        Assert.Equal(departmentId, user.DepartmentId);
        Assert.Equal(level, user.OfficialJurisdictionLevel);
        Assert.Equal(
            level == OfficialJurisdictionLevel.Province ? jurisdictionId : null,
            user.OfficialProvinceId);
        Assert.Equal(
            level == OfficialJurisdictionLevel.District ? jurisdictionId : null,
            user.OfficialDistrictId);
        Assert.Equal(
            level == OfficialJurisdictionLevel.Neighborhood ? jurisdictionId : null,
            user.OfficialNeighborhoodId);
    }

    [Fact]
    public void UpdateAccess_NonOfficialClearsOfficialAccess()
    {
        var user = new User();
        user.UpdateAccess(
            UserRole.Official,
            Guid.NewGuid(),
            OfficialJurisdictionLevel.District,
            Guid.NewGuid());

        user.UpdateAccess(UserRole.Citizen);

        Assert.Equal(UserRole.Citizen, user.Role);
        Assert.Null(user.DepartmentId);
        Assert.Null(user.OfficialJurisdictionLevel);
        Assert.Null(user.OfficialProvinceId);
        Assert.Null(user.OfficialDistrictId);
        Assert.Null(user.OfficialNeighborhoodId);
    }

    [Fact]
    public void Validator_RejectsOfficialWithoutDepartmentAndJurisdiction()
    {
        var validator = new UpdateUserAccessCommandValidator();
        var command = new UpdateUserAccessCommand(
            Guid.NewGuid(),
            UserRole.Official,
            null,
            null,
            null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.DepartmentId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.OfficialJurisdictionLevel));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.OfficialJurisdictionId));
    }

    [Fact]
    public void Validator_RejectsJurisdictionForAdmin()
    {
        var validator = new UpdateUserAccessCommandValidator();
        var command = new UpdateUserAccessCommand(
            Guid.NewGuid(),
            UserRole.Admin,
            null,
            OfficialJurisdictionLevel.Province,
            Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
