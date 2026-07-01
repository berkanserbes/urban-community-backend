using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Organizations.Commands.SetOrganizationActive;

public sealed class SetOrganizationActiveCommandHandler(IApplicationDbContext context)
    : IRequestHandler<
        SetOrganizationActiveCommand,
        Result<SetOrganizationActiveCommandResponse>>
{
    public async Task<Result<SetOrganizationActiveCommandResponse>> Handle(
        SetOrganizationActiveCommand request,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.Id);

        if (request.IsActive)
            organization.Activate();
        else
            organization.Deactivate();

        await context.SaveChangesAsync(cancellationToken);

        var response = await context.Organizations
            .AsNoTracking()
            .Where(value => value.Id == organization.Id)
            .Select(value => new SetOrganizationActiveCommandResponse(
                value.Id,
                value.Name,
                value.JurisdictionLevel,
                value.ProvinceId,
                value.Province.Name,
                value.DistrictId,
                value.District != null ? value.District.Name : null,
                value.NeighborhoodId,
                value.Neighborhood != null ? value.Neighborhood.Name : null,
                value.IsActive,
                value.Departments.Count,
                value.Departments.SelectMany(department => department.Officials).Count()))
            .SingleAsync(cancellationToken);

        return Result<SetOrganizationActiveCommandResponse>.Success(response);
    }
}
