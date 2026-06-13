using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Scope)
            .IsInEnum().WithMessage("Geçersiz duyuru kapsamı.");

        RuleFor(x => x)
            .Must(x =>
                x.Scope switch
                {
                    AnnouncementScope.All =>
                        !x.ProvinceId.HasValue && !x.DistrictId.HasValue && !x.NeighborhoodId.HasValue,
                    AnnouncementScope.Province =>
                        x.ProvinceId.HasValue && !x.DistrictId.HasValue && !x.NeighborhoodId.HasValue,
                    AnnouncementScope.District =>
                        !x.ProvinceId.HasValue && x.DistrictId.HasValue && !x.NeighborhoodId.HasValue,
                    AnnouncementScope.Neighborhood =>
                        !x.ProvinceId.HasValue && !x.DistrictId.HasValue && x.NeighborhoodId.HasValue,
                    _ => false
                })
            .WithMessage("Duyuru kapsamı ile yalnızca ilgili lokasyon ID'si gönderilmelidir.");

        RuleFor(x => x.ExpiresAt)
                .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
                .WithMessage("Son geçerlilik tarihi gelecekte olmalıdır.");
    }
}
