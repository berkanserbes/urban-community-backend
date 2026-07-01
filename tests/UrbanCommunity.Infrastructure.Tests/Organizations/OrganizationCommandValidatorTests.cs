using UrbanCommunity.Application.Features.Organizations.Commands.CreateOrganization;
using UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;
using UrbanCommunity.Domain.Enums;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Organizations;

public sealed class OrganizationCommandValidatorTests
{
    [Fact]
    public void CreateValidator_RequiresDistrictAndNeighborhood_ForNeighborhoodScope()
    {
        var validator = new CreateOrganizationCommandValidator();
        var command = new CreateOrganizationCommand(
            "Headman's Office",
            OrganizationJurisdictionLevel.Neighborhood,
            Guid.NewGuid(),
            null,
            null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.DistrictId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.NeighborhoodId));
    }

    [Fact]
    public void UpdateValidator_AcceptsCompleteNeighborhoodScope()
    {
        var validator = new UpdateOrganizationCommandValidator();
        var command = new UpdateOrganizationCommand(
            Guid.NewGuid(),
            "Headman's Office",
            OrganizationJurisdictionLevel.Neighborhood,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
