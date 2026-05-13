using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<GetCategoriesQueryResponse>>>;
