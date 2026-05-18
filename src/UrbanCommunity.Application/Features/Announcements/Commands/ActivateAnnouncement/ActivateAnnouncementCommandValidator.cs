using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAnnouncement;

public sealed class ActivateAnnouncementCommandValidator : AbstractValidator<ActivateAnnouncementCommand>
{
    public ActivateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Son geçerlilik tarihi gelecekte olmalıdır.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
