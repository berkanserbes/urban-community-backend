using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;

public sealed class GetManageableReportsQueryValidator : AbstractValidator<GetManageableReportsQuery>
{
    public GetManageableReportsQueryValidator()
    {
        RuleFor(query => query.RequestingUserId).NotEmpty();

        RuleFor(query => query.NeighborhoodId)
            .NotEmpty()
            .When(query => query.NeighborhoodId.HasValue);

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue);

        RuleFor(query => query.CategoryId)
            .NotEmpty()
            .When(query => query.CategoryId.HasValue);

        RuleFor(query => query.AssignedDepartmentId)
            .NotEmpty()
            .When(query => query.AssignedDepartmentId.HasValue);
    }
}
