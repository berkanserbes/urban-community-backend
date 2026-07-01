using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Commands.CreateOfficial;

public sealed class CreateOfficialCommandValidator : AbstractValidator<CreateOfficialCommand>
{
    public CreateOfficialCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.PhoneNumber).MaximumLength(20);
        RuleFor(command => command.OrganizationDepartmentId).NotEmpty();
        RuleFor(command => command.OfficialJurisdictionLevel).IsInEnum();
        RuleFor(command => command.OfficialJurisdictionId).NotEmpty();
    }
}
