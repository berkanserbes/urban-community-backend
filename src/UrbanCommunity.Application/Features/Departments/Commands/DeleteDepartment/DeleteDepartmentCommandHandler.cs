using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;

public sealed class DeleteDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDepartmentCommand, Result<DeleteDepartmentCommandResponse>>
{
    public async Task<Result<DeleteDepartmentCommandResponse>> Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Department", request.Id);

        var isUsedByOrganization = await context.OrganizationDepartments
            .AsNoTracking()
            .AnyAsync(value => value.DepartmentId == request.Id, cancellationToken);

        if (isUsedByOrganization)
        {
            return Result<DeleteDepartmentCommandResponse>.Failure(
                ["Bu global birim kurumlara bağlı yerel birimlerde kullanıldığı için silinemez."]);
        }

        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteDepartmentCommandResponse>.Success(
            new DeleteDepartmentCommandResponse("Departman başarıyla silindi."));
    }
}
