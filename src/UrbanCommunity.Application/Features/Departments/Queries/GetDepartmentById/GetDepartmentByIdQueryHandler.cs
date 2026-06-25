using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentById;

public sealed class GetDepartmentByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepartmentByIdQuery, Result<GetDepartmentByIdQueryResponse>>
{
    public async Task<Result<GetDepartmentByIdQueryResponse>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new GetDepartmentByIdQueryResponse(
                d.Id,
                d.Name,
                d.Description))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Department), request.Id);

        return Result<GetDepartmentByIdQueryResponse>.Success(department);
    }
}
