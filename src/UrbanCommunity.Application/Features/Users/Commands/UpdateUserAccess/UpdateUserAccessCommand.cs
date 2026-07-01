using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;

public sealed record UpdateUserAccessCommand(
    Guid UserId,
    UserRole Role,
    Guid? OrganizationDepartmentId,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId,
    Guid? RequestingUserId = null)
    : IRequest<Result<UpdateUserAccessCommandResponse>>;
