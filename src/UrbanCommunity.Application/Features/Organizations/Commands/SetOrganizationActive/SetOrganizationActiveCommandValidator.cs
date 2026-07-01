using FluentValidation;

namespace UrbanCommunity.Application.Features.Organizations.Commands.SetOrganizationActive;

public sealed class SetOrganizationActiveCommandValidator
    : AbstractValidator<SetOrganizationActiveCommand>
{
    public SetOrganizationActiveCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
