using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentCategories;

public sealed class SetOrganizationDepartmentCategoriesCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<
        SetOrganizationDepartmentCategoriesCommand,
        Result<SetOrganizationDepartmentCategoriesCommandResponse>>
{
    public async Task<Result<SetOrganizationDepartmentCategoriesCommandResponse>> Handle(
        SetOrganizationDepartmentCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        var organizationDepartment = await context.OrganizationDepartments
            .Include(value => value.Categories)
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationDepartment), request.Id);

        var categoryIds = request.CategoryIds
            .Distinct()
            .ToArray();

        var existingCategoryCount = await context.ReportCategories
            .AsNoTracking()
            .CountAsync(
                category => categoryIds.Contains(category.Id),
                cancellationToken);
        if (existingCategoryCount != categoryIds.Length)
        {
            return Result<SetOrganizationDepartmentCategoriesCommandResponse>.Failure(
                ["Bir veya daha fazla kategori bulunamadı."]);
        }

        var conflicts = await context.OrganizationDepartmentCategories
            .AsNoTracking()
            .Where(link =>
                categoryIds.Contains(link.ReportCategoryId) &&
                link.OrganizationDepartmentId != organizationDepartment.Id &&
                link.OrganizationDepartment.OrganizationId ==
                    organizationDepartment.OrganizationId &&
                link.OrganizationDepartment.IsActive)
            .Select(link => link.ReportCategory.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (conflicts.Count > 0)
        {
            return Result<SetOrganizationDepartmentCategoriesCommandResponse>.Failure(
                [$"Bu kurumda başka aktif birime bağlı kategoriler: {string.Join(", ", conflicts)}"]);
        }

        context.OrganizationDepartmentCategories.RemoveRange(
            organizationDepartment.Categories);

        foreach (var categoryId in categoryIds)
        {
            context.OrganizationDepartmentCategories.Add(
                new OrganizationDepartmentCategory
                {
                    OrganizationDepartmentId = organizationDepartment.Id,
                    ReportCategoryId = categoryId
                });
        }

        await context.SaveChangesAsync(cancellationToken);

        var response = await context.OrganizationDepartments
            .AsNoTracking()
            .Where(value => value.Id == organizationDepartment.Id)
            .Select(value => new SetOrganizationDepartmentCategoriesCommandResponse(
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

        return Result<SetOrganizationDepartmentCategoriesCommandResponse>.Success(response);
    }
}
