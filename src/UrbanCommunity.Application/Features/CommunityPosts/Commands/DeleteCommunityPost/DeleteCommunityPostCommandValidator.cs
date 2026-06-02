using FluentValidation;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;

public sealed class DeleteCommunityPostCommandValidator : AbstractValidator<DeleteCommunityPostCommand>
{
    public DeleteCommunityPostCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
