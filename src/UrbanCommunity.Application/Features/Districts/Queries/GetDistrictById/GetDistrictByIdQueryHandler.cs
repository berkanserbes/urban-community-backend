using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Districts.Queries.GetDistrictById;

public sealed class GetDistrictByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDistrictByIdQuery, Result<GetDistrictByIdQueryResponse>>
{
    public async Task<Result<GetDistrictByIdQueryResponse>> Handle(
        GetDistrictByIdQuery request,
        CancellationToken cancellationToken)
    {
        var district = await context.Districts
            .AsNoTracking()
            .Where(d => d.Id == request.DistrictId)
            .Select(d => new GetDistrictByIdQueryResponse(
                d.Id,
                d.Name,
                d.ProvinceId,
                d.Province.Name))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(District), request.DistrictId);

        return Result<GetDistrictByIdQueryResponse>.Success(district);
    }
}
