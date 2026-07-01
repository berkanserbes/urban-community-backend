using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;

public sealed class UpdateUserAccessCommandValidator : AbstractValidator<UpdateUserAccessCommand>
{
    public UpdateUserAccessCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role).IsInEnum();

        When(command => command.Role == UserRole.Official, () =>
        {
            RuleFor(command => command.OrganizationDepartmentId)
                .NotEmpty()
                .WithMessage("Official rolü için departman zorunludur.");

            RuleFor(command => command.OfficialJurisdictionLevel)
                .NotNull()
                .IsInEnum()
                .WithMessage("Official rolü için geçerli bir yetki alanı seviyesi zorunludur.");

            RuleFor(command => command.OfficialJurisdictionId)
                .NotEmpty()
                .WithMessage("Official rolü için yetki alanı zorunludur.");
        });

        When(command => command.Role != UserRole.Official, () =>
        {
            RuleFor(command => command.OrganizationDepartmentId)
                .Null()
                .WithMessage("Departman yalnızca Official rolü için atanabilir.");

            RuleFor(command => command.OfficialJurisdictionLevel)
                .Null()
                .WithMessage("Yetki alanı seviyesi yalnızca Official rolü için atanabilir.");

            RuleFor(command => command.OfficialJurisdictionId)
                .Null()
                .WithMessage("Yetki alanı yalnızca Official rolü için atanabilir.");
        });
    }
}
