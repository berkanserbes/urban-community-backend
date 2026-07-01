using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Organizations.Commands.DeleteOrganization;

public sealed record DeleteOrganizationCommand(Guid Id)
    : IRequest<Result<DeleteOrganizationCommandResponse>>;
