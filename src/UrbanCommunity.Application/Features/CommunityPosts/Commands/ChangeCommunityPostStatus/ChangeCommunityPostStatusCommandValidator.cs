using FluentValidation;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;

public sealed class ChangeCommunityPostStatusCommandValidator : AbstractValidator<ChangeCommunityPostStatusCommand>
{
    public ChangeCommunityPostStatusCommandValidator()
    {
        RuleFor(x => x.CommunityPostId)
            .NotEmpty().WithMessage("Topluluk gönderisi ID zorunludur.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Geçersiz topluluk gönderisi durumu.");
    }
}
