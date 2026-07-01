using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string Name,
    OrganizationJurisdictionLevel JurisdictionLevel,
    Guid ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId) : IRequest<Result<CreateOrganizationCommandResponse>>;
