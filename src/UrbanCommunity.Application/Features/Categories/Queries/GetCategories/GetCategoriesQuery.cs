using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery
    : IRequest<Result<IReadOnlyList<GetCategoriesQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.CategoriesList;
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
