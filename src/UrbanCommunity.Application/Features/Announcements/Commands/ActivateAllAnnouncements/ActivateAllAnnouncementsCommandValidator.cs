using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAllAnnouncements;

public sealed class ActivateAllAnnouncementsCommandValidator : AbstractValidator<ActivateAllAnnouncementsCommand>
{
    public ActivateAllAnnouncementsCommandValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Son geçerlilik tarihi gelecekte olmalıdır.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
