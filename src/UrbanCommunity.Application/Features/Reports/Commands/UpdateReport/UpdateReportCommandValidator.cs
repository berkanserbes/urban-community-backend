using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;

public sealed class UpdateReportCommandValidator : AbstractValidator<UpdateReportCommand>
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

    public UpdateReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği gereklidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MinimumLength(3).WithMessage("Başlık en az 3 karakter olmalıdır.")
            .MaximumLength(150).WithMessage("Başlık en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz.")
            .MinimumLength(10).WithMessage("Açıklama en az 10 karakter olmalıdır.")
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");

        RuleFor(x => x)
            .Must(HasCompleteLocation)
            .WithMessage("Enlem ve boylam birlikte gönderilmeli veya ikisi de boş bırakılmalıdır.");

        RuleFor(x => x.Latitude)
            .Cascade(CascadeMode.Stop)
            .Must(value => value is null || double.IsFinite(value.Value)).WithMessage("Enlem geçerli bir sayı olmalıdır.")
            .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .Cascade(CascadeMode.Stop)
            .Must(value => value is null || double.IsFinite(value.Value)).WithMessage("Boylam geçerli bir sayı olmalıdır.")
            .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır.")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.SuggestedCategoryId)
            .NotEmpty().WithMessage("Önerilen kategori kimliği boş olamaz.")
            .When(x => x.SuggestedCategoryId.HasValue);

        RuleFor(x => x.NeighborhoodId)
            .NotEmpty().WithMessage("Mahalle seçilmelidir.");

        RuleFor(x => x.DeleteMediaIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("Silinecek medya kimlikleri tekrar edemez.");

        RuleFor(x => x.Media)
            .Must(media => media is null || media.Count <= 10)
            .WithMessage("Tek istekte en fazla 10 medya dosyası yüklenebilir.");

        RuleForEach(x => x.Media)
            .Must(HaveAllowedSize).WithMessage("Her medya dosyası en fazla 50 MB olabilir.")
            .Must(HaveAllowedExtension).WithMessage("Sadece jpg, jpeg, png, webp, mp4, webm veya mov dosyaları yüklenebilir.")
            .Must(HaveAllowedContentType).WithMessage("Desteklenmeyen medya türü.");
    }

    private static bool HaveAllowedSize(IFormFile? file) =>
        file is not null && file.Length > 0 && file.Length <= 50 * 1024 * 1024;

    private static bool HaveAllowedExtension(IFormFile? file)
    {
        if (file is null)
            return false;

        return AllowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase);
    }

    private static bool HaveAllowedContentType(IFormFile? file) =>
        file is not null && AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);

    private static bool HasCompleteLocation(UpdateReportCommand command) =>
        command.Latitude.HasValue == command.Longitude.HasValue;
}
