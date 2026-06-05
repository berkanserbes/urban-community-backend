using FluentValidation;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPostById;

public sealed class GetCommunityPostByIdQueryValidator : AbstractValidator<GetCommunityPostByIdQuery>
{
    public GetCommunityPostByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Topluluk gönderisi ID zorunludur.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("İstek yapan kullanıcı ID zorunludur.");
    }
}
