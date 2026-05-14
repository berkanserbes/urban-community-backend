using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;

public sealed class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Profil resmi boş olamaz.")
            .Must(HaveAllowedExtension)
            .WithMessage("Sadece .jpg, .jpeg, .png veya .webp formatındaki resimler yüklenebilir.")
            .Must(HaveAllowedContentType)
            .WithMessage("Desteklenmeyen dosya tipi.")
            .Must(BeWithinSizeLimit)
            .WithMessage("Profil resmi 5 MB'dan büyük olamaz.");
    }

    private bool HaveAllowedExtension(IFormFile file)
    {
        if (file == null) return false;
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private bool HaveAllowedContentType(IFormFile file)
    {
        if (file == null) return false;
        return AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    private bool BeWithinSizeLimit(IFormFile file)
    {
        if (file == null) return false;
        return file.Length <= MaxFileSize;
    }
}
