using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");

        RuleFor(x => x)
                .Must(x =>
                    x.Scope switch
                    {
                        AnnouncementScope.Province => x.ProvinceId.HasValue,
                        AnnouncementScope.District => x.DistrictId.HasValue,
                        AnnouncementScope.Neighborhood => x.NeighborhoodId.HasValue,
                        _ => true
                    })
                .WithMessage("Scope ile uyumlu ID zorunludur.");

        RuleFor(x => x.ExpiresAt)
                .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
                .WithMessage("Son geçerlilik tarihi gelecekte olmalıdır.");
    }
}
