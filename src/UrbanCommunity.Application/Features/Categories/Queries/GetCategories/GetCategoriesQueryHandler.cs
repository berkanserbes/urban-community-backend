using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<GetCategoriesQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetCategoriesQueryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await context.ReportCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new GetCategoriesQueryResponse(c.Id, c.Name, c.Description, c.IconUrl))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetCategoriesQueryResponse>>.Success(categories);
    }
}
