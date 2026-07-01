using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Organizations.Commands.DeleteOrganization;

public sealed class DeleteOrganizationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteOrganizationCommand, Result<DeleteOrganizationCommandResponse>>
{
    public async Task<Result<DeleteOrganizationCommandResponse>> Handle(
        DeleteOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var organization = await context.Organizations
            .FirstOrDefaultAsync(value => value.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Organization), request.Id);

        var hasDepartments = await context.OrganizationDepartments
            .AsNoTracking()
            .AnyAsync(value => value.OrganizationId == request.Id, cancellationToken);

        if (hasDepartments)
        {
            return Result<DeleteOrganizationCommandResponse>.Failure(
                ["Kurumun yerel birimleri bulunduğu için silinemez. Önce yerel birimleri silin."]);
        }

        context.Organizations.Remove(organization);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteOrganizationCommandResponse>.Success(
            new DeleteOrganizationCommandResponse("Kurum başarıyla silindi."));
    }
}
