using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.NeighborhoodId)
            .NotEmpty().WithMessage("Mahalle kimliği boş olamaz.")
            .When(x => x.NeighborhoodId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçerli bir rapor durumu seçilmelidir.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori kimliği boş olamaz.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.AssignedDepartmentId)
            .NotEmpty().WithMessage("Birim kimliği boş olamaz.")
            .When(x => x.AssignedDepartmentId.HasValue);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.")
            .When(x => x.UserId.HasValue);
    }
}
