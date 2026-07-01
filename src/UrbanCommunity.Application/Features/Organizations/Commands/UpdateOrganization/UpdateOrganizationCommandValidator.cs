using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandValidator
    : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.JurisdictionLevel)
            .IsInEnum();

        RuleFor(command => command.ProvinceId)
            .NotEmpty();

        RuleFor(command => command.DistrictId)
            .NotEmpty()
            .When(command =>
                command.JurisdictionLevel is
                    OrganizationJurisdictionLevel.District or
                    OrganizationJurisdictionLevel.Neighborhood);

        RuleFor(command => command.DistrictId)
            .Null()
            .When(command =>
                command.JurisdictionLevel == OrganizationJurisdictionLevel.Province);

        RuleFor(command => command.NeighborhoodId)
            .NotEmpty()
            .When(command =>
                command.JurisdictionLevel == OrganizationJurisdictionLevel.Neighborhood);

        RuleFor(command => command.NeighborhoodId)
            .Null()
            .When(command =>
                command.JurisdictionLevel != OrganizationJurisdictionLevel.Neighborhood);
    }
}
