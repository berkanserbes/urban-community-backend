using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryByIdQueryResponse>>
{
    public async Task<Result<GetCategoryByIdQueryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await context.ReportCategories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new GetCategoryByIdQueryResponse(c.Id, c.Name, c.Description, c.IconUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("ReportCategory", request.Id);
        }

        return Result<GetCategoryByIdQueryResponse>.Success(category);
    }
}
