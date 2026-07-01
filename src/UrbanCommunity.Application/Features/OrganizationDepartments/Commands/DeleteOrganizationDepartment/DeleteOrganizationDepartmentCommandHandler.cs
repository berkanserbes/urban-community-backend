using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.DeleteOrganizationDepartment;

public sealed class DeleteOrganizationDepartmentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<
        DeleteOrganizationDepartmentCommand,
        Result<DeleteOrganizationDepartmentCommandResponse>>
{
    public async Task<Result<DeleteOrganizationDepartmentCommandResponse>> Handle(
        DeleteOrganizationDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var organizationDepartment = await context.OrganizationDepartments
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationDepartment), request.Id);

        var hasOfficials = await context.Users
            .AsNoTracking()
            .AnyAsync(value => value.OrganizationDepartmentId == request.Id, cancellationToken);

        if (hasOfficials)
        {
            return Result<DeleteOrganizationDepartmentCommandResponse>.Failure(
                ["Yerel birime atanmış resmi kullanıcılar bulunduğu için silinemez."]);
        }

        var hasReports = await context.Reports
            .AsNoTracking()
            .AnyAsync(value => value.AssignedOrganizationDepartmentId == request.Id, cancellationToken);

        if (hasReports)
        {
            return Result<DeleteOrganizationDepartmentCommandResponse>.Failure(
                ["Yerel birime atanmış raporlar bulunduğu için silinemez."]);
        }

        context.OrganizationDepartments.Remove(organizationDepartment);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteOrganizationDepartmentCommandResponse>.Success(
            new DeleteOrganizationDepartmentCommandResponse("Yerel birim başarıyla silindi."));
    }
}
