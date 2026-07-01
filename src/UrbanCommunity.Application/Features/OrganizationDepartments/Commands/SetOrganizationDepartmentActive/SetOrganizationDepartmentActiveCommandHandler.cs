using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentActive;

public sealed class SetOrganizationDepartmentActiveCommandHandler(IApplicationDbContext context)
    : IRequestHandler<
        SetOrganizationDepartmentActiveCommand,
        Result<SetOrganizationDepartmentActiveCommandResponse>>
{
    public async Task<Result<SetOrganizationDepartmentActiveCommandResponse>> Handle(
        SetOrganizationDepartmentActiveCommand request,
        CancellationToken cancellationToken)
    {
        var organizationDepartment = await context.OrganizationDepartments
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationDepartment), request.Id);

        if (request.IsActive)
            organizationDepartment.Activate();
        else
            organizationDepartment.Deactivate();

        await context.SaveChangesAsync(cancellationToken);

        var response = await context.OrganizationDepartments
            .AsNoTracking()
            .Where(value => value.Id == organizationDepartment.Id)
            .Select(value => new SetOrganizationDepartmentActiveCommandResponse(
                value.Id,
                value.OrganizationId,
                value.Organization.Name,
                value.DepartmentId,
                value.Department.Name,
                value.DisplayName ?? value.Department.Name,
                value.ContactEmail,
                value.ContactPhone,
                value.IsActive,
                value.Categories.Select(link => link.ReportCategoryId).ToList(),
                value.Officials.Count,
                value.AssignedReports.Count))
            .SingleAsync(cancellationToken);

        return Result<SetOrganizationDepartmentActiveCommandResponse>.Success(response);
    }
}
