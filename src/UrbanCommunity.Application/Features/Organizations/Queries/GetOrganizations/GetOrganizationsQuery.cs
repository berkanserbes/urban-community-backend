using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizations;

public sealed record GetOrganizationsQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<GetOrganizationsQueryResponse>>>;
