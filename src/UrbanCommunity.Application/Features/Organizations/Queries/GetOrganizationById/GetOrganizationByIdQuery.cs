using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(Guid Id)
    : IRequest<Result<GetOrganizationByIdQueryResponse>>;
