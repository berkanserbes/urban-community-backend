using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartments;

public sealed class GetDepartmentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyList<GetDepartmentsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetDepartmentsQueryResponse>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new GetDepartmentsQueryResponse(d.Id, d.Name, d.Description))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetDepartmentsQueryResponse>>.Success(departments);
    }
}
