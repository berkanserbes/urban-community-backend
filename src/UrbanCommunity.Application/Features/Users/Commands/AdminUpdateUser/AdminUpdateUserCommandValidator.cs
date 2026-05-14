using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserCommandValidator : AbstractValidator<AdminUpdateUserCommand>
{
    public AdminUpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Bio).MaximumLength(500).When(x => x.Bio is not null);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Geçersiz telefon numarası formatı.");
    }
}
