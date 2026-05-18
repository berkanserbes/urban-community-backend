using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinces;

public sealed record GetProvincesQuery : IRequest<Result<IReadOnlyList<GetProvincesQueryResponse>>>;
