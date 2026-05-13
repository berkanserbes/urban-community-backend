using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoriesWithReportCount;

public sealed class GetCategoriesWithReportCountQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesWithReportCountQuery, Result<IReadOnlyList<GetCategoriesWithReportCountQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetCategoriesWithReportCountQueryResponse>>> Handle(
        GetCategoriesWithReportCountQuery request, CancellationToken cancellationToken)
    {
        var categories = await context.ReportCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new GetCategoriesWithReportCountQueryResponse(
                c.Id,
                c.Name,
                c.Description,
                c.IconUrl,
                c.Reports.Count))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetCategoriesWithReportCountQueryResponse>>.Success(categories);
    }
}
