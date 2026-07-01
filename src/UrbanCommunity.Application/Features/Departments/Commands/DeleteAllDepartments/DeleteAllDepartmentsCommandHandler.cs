using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteAllDepartments;

public sealed class DeleteAllDepartmentsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAllDepartmentsCommand, Result<DeleteAllDepartmentsCommandResponse>>
{
    public async Task<Result<DeleteAllDepartmentsCommandResponse>> Handle(
        DeleteAllDepartmentsCommand request,
        CancellationToken cancellationToken)
    {
        var departments = await context.Departments.ToListAsync(cancellationToken);

        if (departments.Count == 0)
        {
            return Result<DeleteAllDepartmentsCommandResponse>.Failure(
                ["Silinecek birim bulunamadı."]);
        }

        var departmentsUsedByOrganizations = await context.Departments
            .Where(department => context.OrganizationDepartments
                .AsNoTracking()
                .Any(localDepartment => localDepartment.DepartmentId == department.Id))
            .ToListAsync(cancellationToken);

        if (departmentsUsedByOrganizations.Count > 0)
        {
            var departmentNames = string.Join(
                ", ",
                departmentsUsedByOrganizations.Select(department => department.Name));

            return Result<DeleteAllDepartmentsCommandResponse>.Failure(
                [$"Kurumlara bağlı yerel birimlerde kullanılan global birimler silinemez: {departmentNames}"]);
        }

        context.Departments.RemoveRange(departments);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAllDepartmentsCommandResponse>.Success(
            new DeleteAllDepartmentsCommandResponse(
                "Tüm birimler başarıyla silindi.",
                departments.Count));
    }
}
