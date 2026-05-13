using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDepartmentCommand, Result<UpdateDepartmentCommandResponse>>
{
    public async Task<Result<UpdateDepartmentCommandResponse>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department is null)
        {
            throw new NotFoundException("Department", request.Id);
        }

        var nameConflict = await context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Name == request.Name.Trim() && d.Id != request.Id, cancellationToken);

        if (nameConflict)
        {
            throw new AlreadyExistsException($"Bu isimde bir birim zaten mevcut: {request.Name}");
        }

        department.UpdateDetails(request.Name, request.Description, request.ContactEmail, request.ContactPhone);
        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateDepartmentCommandResponse>.Success(new UpdateDepartmentCommandResponse(
            department.Id, department.Name, department.Description, department.ContactEmail, department.ContactPhone));
    }
}
