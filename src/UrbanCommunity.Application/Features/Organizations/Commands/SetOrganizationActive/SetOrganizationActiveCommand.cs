using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Organizations.Commands.SetOrganizationActive;

public sealed record SetOrganizationActiveCommand(
    Guid Id,
    bool IsActive) : IRequest<Result<SetOrganizationActiveCommandResponse>>;
