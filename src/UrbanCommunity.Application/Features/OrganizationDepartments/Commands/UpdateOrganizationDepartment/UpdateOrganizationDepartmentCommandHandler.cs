using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.UpdateOrganizationDepartment;

public sealed class UpdateOrganizationDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<
        UpdateOrganizationDepartmentCommand,
        Result<UpdateOrganizationDepartmentCommandResponse>>
{
    public async Task<Result<UpdateOrganizationDepartmentCommandResponse>> Handle(
        UpdateOrganizationDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var organizationDepartment = await context.OrganizationDepartments
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationDepartment), request.Id);

        organizationDepartment.UpdateDetails(
            request.DisplayName,
            request.ContactEmail,
            request.ContactPhone);
        await context.SaveChangesAsync(cancellationToken);

        var response = await context.OrganizationDepartments
            .AsNoTracking()
            .Where(value => value.Id == organizationDepartment.Id)
            .Select(value => new UpdateOrganizationDepartmentCommandResponse(
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

        return Result<UpdateOrganizationDepartmentCommandResponse>.Success(response);
    }
}
