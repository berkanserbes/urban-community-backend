using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.Scope)
            .Must(s => s is null || s is "Neighborhood" or "District" or "Province")
            .WithMessage("Geçerli bir kapsam seçilmelidir (Neighborhood, District, Province).");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçerli bir rapor durumu seçilmelidir.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori kimliği boş olamaz.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.AssignedOrganizationDepartmentId)
            .NotEmpty().WithMessage("Birim kimliği boş olamaz.")
            .When(x => x.AssignedOrganizationDepartmentId.HasValue);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.")
            .When(x => x.UserId.HasValue);
    }
}
