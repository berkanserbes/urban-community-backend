using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDepartmentCommand, Result<CreateDepartmentCommandResponse>>
{
    public async Task<Result<CreateDepartmentCommandResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Name == request.Name.Trim(), cancellationToken);

        if (nameExists)
        {
            throw new AlreadyExistsException($"Bu isimde bir birim zaten mevcut: {request.Name}");
        }

        var department = new Department
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync(cancellationToken);

        return Result<CreateDepartmentCommandResponse>.Success(new CreateDepartmentCommandResponse(
            department.Id, department.Name, department.Description));
    }
}
