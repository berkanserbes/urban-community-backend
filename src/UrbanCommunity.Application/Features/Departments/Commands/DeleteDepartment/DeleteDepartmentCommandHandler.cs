using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;

public sealed class DeleteDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDepartmentCommand, Result<DeleteDepartmentCommandResponse>>
{
    public async Task<Result<DeleteDepartmentCommandResponse>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department is null)
        {
            throw new NotFoundException("Department", request.Id);
        }

        var hasOfficials = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.DepartmentId == request.Id, cancellationToken);

        if (hasOfficials)
        {
            return Result<DeleteDepartmentCommandResponse>.Failure(["Bu birime atanış personel bulunduğu için silinemez."]);
        }

        var hasReports = await context.Reports
            .AsNoTracking()
            .AnyAsync(r => r.AssignedDepartmentId == request.Id, cancellationToken);

        if (hasReports)
        {
            return Result<DeleteDepartmentCommandResponse>.Failure(["Bu birime atanış raporlar bulunduğu için silinemez."]);
        }

        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteDepartmentCommandResponse>.Success(
            new DeleteDepartmentCommandResponse("Departman başarıyla silindi."));
    }
}
