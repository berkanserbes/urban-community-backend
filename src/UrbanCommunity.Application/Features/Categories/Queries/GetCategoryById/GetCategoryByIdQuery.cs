using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<GetCategoryByIdQueryResponse>>;
