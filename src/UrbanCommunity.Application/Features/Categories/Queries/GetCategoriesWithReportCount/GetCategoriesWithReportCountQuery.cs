using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoriesWithReportCount;

public sealed record GetCategoriesWithReportCountQuery : IRequest<Result<IReadOnlyList<GetCategoriesWithReportCountQueryResponse>>>;
