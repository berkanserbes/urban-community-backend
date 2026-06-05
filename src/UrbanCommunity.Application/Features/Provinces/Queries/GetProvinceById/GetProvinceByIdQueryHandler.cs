using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinceById;

public sealed class GetProvinceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProvinceByIdQuery, Result<GetProvinceByIdQueryResponse>>
{
    public async Task<Result<GetProvinceByIdQueryResponse>> Handle(
        GetProvinceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var province = await context.Provinces
            .AsNoTracking()
            .Where(p => p.Id == request.ProvinceId)
            .Select(p => new GetProvinceByIdQueryResponse(p.Id, p.Name))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Province), request.ProvinceId);

        return Result<GetProvinceByIdQueryResponse>.Success(province);
    }
}
