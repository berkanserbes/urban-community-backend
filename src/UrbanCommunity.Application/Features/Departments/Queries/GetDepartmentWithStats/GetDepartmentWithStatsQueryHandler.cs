using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentWithStats;

public sealed class GetDepartmentWithStatsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepartmentWithStatsQuery, Result<IReadOnlyList<GetDepartmentWithStatsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetDepartmentWithStatsQueryResponse>>> Handle(
        GetDepartmentWithStatsQuery request, CancellationToken cancellationToken)
    {
        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new GetDepartmentWithStatsQueryResponse(
                d.Id,
                d.Name,
                d.Description,
                d.ContactEmail,
                d.ContactPhone,
                d.Officials.Count,
                d.AssignedReports.Count))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetDepartmentWithStatsQueryResponse>>.Success(departments);
    }
}
