using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.AssignRole;

public sealed class AssignRoleCommandHandler(UserManager<User> userManager, IApplicationDbContext context)
    : IRequestHandler<AssignRoleCommand, Result<AssignRoleCommandResponse>>
{
    public async Task<Result<AssignRoleCommandResponse>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }

        user.ChangeRole(request.Role);

        if (request.Role == UserRole.Official && request.DepartmentId.HasValue)
        {
            var departmentExists = await context.Departments
                .AsNoTracking()
                .AnyAsync(d => d.Id == request.DepartmentId.Value, cancellationToken);

            if (!departmentExists)
                throw new NotFoundException(nameof(Department), request.DepartmentId.Value);

            user.AssignDepartment(request.DepartmentId);
        }

        await userManager.UpdateAsync(user);

        return Result<AssignRoleCommandResponse>.Success(new AssignRoleCommandResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.Role,
            user.DepartmentId));
    }
}
