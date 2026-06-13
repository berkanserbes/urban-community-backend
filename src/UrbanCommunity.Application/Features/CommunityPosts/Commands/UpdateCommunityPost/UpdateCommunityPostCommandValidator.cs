using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;

public sealed class UpdateCommunityPostCommandValidator : AbstractValidator<UpdateCommunityPostCommand>
{
    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".mp4", ".webm", ".mov"];

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

    public UpdateCommunityPostCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Topluluk gönderisi ID zorunludur.");

        RuleFor(command => command.RequestingUserId)
            .NotEmpty().WithMessage("İstek yapan kullanıcı ID zorunludur.");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(command => command.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");

        RuleFor(command => command.KeepMediaIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("Korunacak medya ID listesinde tekrarlanan değerler olamaz.");

        RuleForEach(command => command.KeepMediaIds)
            .NotEmpty()
            .WithMessage("Korunacak medya ID'si geçersiz.");

        RuleFor(command => command.Media)
            .Must(media => media is null || media.Count <= MaxMediaCount)
            .WithMessage($"En fazla {MaxMediaCount} medya dosyası yüklenebilir.");

        RuleForEach(command => command.Media)
            .NotNull().WithMessage("Medya dosyası boş olamaz.")
            .Must(HaveContent).WithMessage("Medya dosyası boş olamaz.")
            .Must(HaveAllowedExtension)
            .WithMessage("Sadece .jpg, .jpeg, .png, .webp, .mp4, .webm veya .mov dosyaları yüklenebilir.")
            .Must(HaveAllowedContentType).WithMessage("Desteklenmeyen medya dosyası tipi.")
            .Must(BeWithinSizeLimit).WithMessage("Her medya dosyası en fazla 50 MB olabilir.");
    }

    private static bool HaveContent(IFormFile? file) => file is { Length: > 0 };

    private static bool HaveAllowedExtension(IFormFile? file)
    {
        return file is not null &&
               AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());
    }

    private static bool HaveAllowedContentType(IFormFile? file)
    {
        return file is not null &&
               AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    private static bool BeWithinSizeLimit(IFormFile? file)
    {
        return file is not null && file.Length <= MaxMediaFileSize;
    }
}
