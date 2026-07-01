using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    Guid Id,
    string Name,
    OrganizationJurisdictionLevel JurisdictionLevel,
    Guid ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId) : IRequest<Result<UpdateOrganizationCommandResponse>>;
