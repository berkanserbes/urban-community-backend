using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteAllDepartments;

public sealed class DeleteAllDepartmentsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAllDepartmentsCommand, Result<DeleteAllDepartmentsCommandResponse>>
{
    public async Task<Result<DeleteAllDepartmentsCommandResponse>> Handle(DeleteAllDepartmentsCommand request, CancellationToken cancellationToken)
    {
        var departments = await context.Departments.ToListAsync(cancellationToken);

        if (departments.Count == 0)
            return Result<DeleteAllDepartmentsCommandResponse>.Failure(["Silinecek birim bulunamadı."]);

        var departmentsWithAssignedReports = await context.Departments
            .Where(d => context.Reports.AsNoTracking().Any(r => r.AssignedDepartmentId == d.Id))
            .ToListAsync(cancellationToken);

        if (departmentsWithAssignedReports.Count > 0)
        {
            var departmentNames = string.Join(", ", departmentsWithAssignedReports.Select(d => d.Name));
            return Result<DeleteAllDepartmentsCommandResponse>.Failure(
                [$"Bu birimlere ait ihbarlar bulunduğu için silinemez: {departmentNames}"]);
        }

        context.Departments.RemoveRange(departments);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAllDepartmentsCommandResponse>.Success(
            new DeleteAllDepartmentsCommandResponse(
                "Tüm birimler başarıyla silindi.",
                departments.Count));
    }
}
