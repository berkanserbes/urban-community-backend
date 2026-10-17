using FluentValidation;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Common.Validators;

public sealed class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(request => request.PageIndex)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageIndex en az 1 olmalıdır.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize)
            .WithMessage($"PageSize 1 ile {PaginationRequest.MaxPageSize} arasında olmalıdır.");

        RuleFor(request => request.PageIndex)
            .Must((request, pageIndex) => ((long)pageIndex - 1) * request.PageSize <= int.MaxValue)
            .When(request => request.PageIndex >= 1 && request.PageSize is >= 1 and <= PaginationRequest.MaxPageSize)
            .WithMessage("İstenen sayfa, desteklenen kayıt aralığını aşıyor.");
    }
}
