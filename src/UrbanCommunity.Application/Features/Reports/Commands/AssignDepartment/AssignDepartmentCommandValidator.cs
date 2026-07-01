using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Commands.AssignDepartment;

public sealed class AssignDepartmentCommandValidator : AbstractValidator<AssignDepartmentCommand>
{
    public AssignDepartmentCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.OrganizationDepartmentId)
            .NotEmpty().WithMessage("Birim kimliği boş olamaz.")
            .When(x => x.OrganizationDepartmentId.HasValue);

        RuleFor(x => x.ChangedByUserId)
            .NotEmpty().WithMessage("İşlemi yapan kullanıcı kimliği gereklidir.");
    }
}
