using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.CreateOfficial;

public sealed record CreateOfficialCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid OrganizationDepartmentId,
    OfficialJurisdictionLevel OfficialJurisdictionLevel,
    Guid OfficialJurisdictionId) : IRequest<Result<CreateOfficialCommandResponse>>;
