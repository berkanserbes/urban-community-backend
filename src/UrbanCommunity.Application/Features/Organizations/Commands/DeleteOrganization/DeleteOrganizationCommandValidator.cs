using FluentValidation;

namespace UrbanCommunity.Application.Features.Organizations.Commands.DeleteOrganization;

public sealed class DeleteOrganizationCommandValidator
    : AbstractValidator<DeleteOrganizationCommand>
{
    public DeleteOrganizationCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
