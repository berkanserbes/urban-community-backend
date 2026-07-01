using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.CreateOrganizationDepartment;

public sealed class CreateOrganizationDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<
        CreateOrganizationDepartmentCommand,
        Result<CreateOrganizationDepartmentCommandResponse>>
{
    public async Task<Result<CreateOrganizationDepartmentCommandResponse>> Handle(
        CreateOrganizationDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var organizationIsActive = await context.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization =>
                    organization.Id == request.OrganizationId &&
                    organization.IsActive,
                cancellationToken);
        if (!organizationIsActive)
            throw new NotFoundException(nameof(Organization), request.OrganizationId);

        var departmentExists = await context.Departments
            .AsNoTracking()
            .AnyAsync(
                department => department.Id == request.DepartmentId,
                cancellationToken);
        if (!departmentExists)
            throw new NotFoundException(nameof(Department), request.DepartmentId);

        var localDepartmentExists = await context.OrganizationDepartments
            .AsNoTracking()
            .AnyAsync(
                value =>
                    value.OrganizationId == request.OrganizationId &&
                    value.DepartmentId == request.DepartmentId,
                cancellationToken);
        if (localDepartmentExists)
            throw new AlreadyExistsException("Bu departman kurumda zaten tanımlı.");

        var organizationDepartment = new OrganizationDepartment
        {
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId
        };
        organizationDepartment.UpdateDetails(
            request.DisplayName,
            request.ContactEmail,
            request.ContactPhone);

        context.OrganizationDepartments.Add(organizationDepartment);
        await context.SaveChangesAsync(cancellationToken);

        var response = await context.OrganizationDepartments
            .AsNoTracking()
            .Where(value => value.Id == organizationDepartment.Id)
            .Select(value => new CreateOrganizationDepartmentCommandResponse(
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

        return Result<CreateOrganizationDepartmentCommandResponse>.Success(response);
    }
}
