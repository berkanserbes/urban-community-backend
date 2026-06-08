using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed class GetReportsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportsQuery, Result<IReadOnlyList<GetReportsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetReportsQueryResponse>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Reports
            .AsNoTracking()
            .AsQueryable();

        if (request.NeighborhoodId.HasValue)
            query = query.Where(r => r.NeighborhoodId == request.NeighborhoodId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);

        if (request.AssignedDepartmentId.HasValue)
            query = query.Where(r => r.AssignedDepartmentId == request.AssignedDepartmentId.Value);

        if (request.UserId.HasValue)
            query = query.Where(r => r.UserId == request.UserId.Value);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetReportsQueryResponse(
                r.Id,
                r.Title,
                r.Description,
                r.Status,
                r.Priority,
                r.Location.Y,
                r.Location.X,
                r.UserId,
                r.CategoryId,
                r.Category.Name,
                r.NeighborhoodId,
                r.Neighborhood.Name,
                r.AssignedDepartmentId,
                r.AssignedDepartment != null ? r.AssignedDepartment.Name : null,
                r.CreatedAt,
                r.ResolvedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetReportsQueryResponse>>.Success(reports);
    }
}
