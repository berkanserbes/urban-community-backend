using FluentValidation;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;

public sealed class CreateCommunityPostCommandValidator : AbstractValidator<CreateCommunityPostCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".mp4", ".webm", ".mov"];
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "video/mp4",
        "video/webm",
        "video/quicktime"
    ];

    private const int MaxMediaCount = 10;
    private const long MaxMediaFileSize = 50 * 1024 * 1024;

    public CreateCommunityPostCommandValidator()
    {
        RuleFor(x => x.Type).IsInEnum()
            .WithMessage("Geçerli bir gönderi türü seçiniz.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Media)
            .Must(media => media is null || media.Count <= MaxMediaCount)
            .WithMessage($"En fazla {MaxMediaCount} medya dosyası yüklenebilir.");

        RuleForEach(x => x.Media)
            .NotNull().WithMessage("Medya dosyası boş olamaz.")
            .Must(HaveContent).WithMessage("Medya dosyası boş olamaz.")
            .Must(HaveAllowedExtension).WithMessage("Sadece .jpg, .jpeg, .png, .webp, .mp4, .webm veya .mov dosyaları yüklenebilir.")
            .Must(HaveAllowedContentType).WithMessage("Desteklenmeyen medya dosyası tipi.")
            .Must(BeWithinSizeLimit).WithMessage("Her medya dosyası en fazla 50 MB olabilir.");

        RuleFor(x => x.UserId).NotEmpty()
            .WithMessage("Kullanıcı kimliği gereklidir.");

        RuleFor(x => x.Scope).IsInEnum()
            .WithMessage("Geçerli bir gönderi kapsamı seçiniz.");

        RuleFor(x => x).Custom((command, context) =>
        {
            switch (command.Scope)
            {
                case CommunityPostScope.Province:
                    if (!command.ProvinceId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.ProvinceId), "İl kimliği gereklidir.");
                    }

                    if (command.DistrictId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.DistrictId), "İl kapsamındaki gönderide ilçe kimliği gönderilmemelidir.");
                    }

                    if (command.NeighborhoodId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.NeighborhoodId), "İl kapsamındaki gönderide mahalle kimliği gönderilmemelidir.");
                    }
                    break;

                case CommunityPostScope.District:
                    if (!command.DistrictId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.DistrictId), "İlçe kimliği gereklidir.");
                    }

                    if (command.ProvinceId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.ProvinceId), "İlçe kapsamındaki gönderide il kimliği gönderilmemelidir.");
                    }

                    if (command.NeighborhoodId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.NeighborhoodId), "İlçe kapsamındaki gönderide mahalle kimliği gönderilmemelidir.");
                    }
                    break;

                case CommunityPostScope.Neighborhood:
                    if (!command.NeighborhoodId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.NeighborhoodId), "Mahalle kimliği gereklidir.");
                    }

                    if (command.ProvinceId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.ProvinceId), "Mahalle kapsamındaki gönderide il kimliği gönderilmemelidir.");
                    }

                    if (command.DistrictId.HasValue)
                    {
                        context.AddFailure(nameof(CreateCommunityPostCommand.DistrictId), "Mahalle kapsamındaki gönderide ilçe kimliği gönderilmemelidir.");
                    }
                    break;
            }
        });
    }

    private static bool HaveContent(IFormFile? file)
    {
        return file is { Length: > 0 };
    }

    private static bool HaveAllowedExtension(IFormFile? file)
    {
        if (file is null)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private static bool HaveAllowedContentType(IFormFile? file)
    {
        if (file is null)
            return false;

        return AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    private static bool BeWithinSizeLimit(IFormFile? file)
    {
        return file is not null && file.Length <= MaxMediaFileSize;
    }
}
