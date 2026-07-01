using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Application.Features.Users;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;

public sealed class UpdateUserAccessCommandHandler(
    IApplicationDbContext context,
    IRefreshTokenRevocationService refreshTokenRevocationService)
    : IRequestHandler<UpdateUserAccessCommand, Result<UpdateUserAccessCommandResponse>>
{
    public async Task<Result<UpdateUserAccessCommandResponse>> Handle(
        UpdateUserAccessCommand request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(user => user.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (request.RequestingUserId == user.Id && request.Role != user.Role)
        {
            return Result<UpdateUserAccessCommandResponse>.Failure(
                ["Admin kendi rolünü değiştiremez."]);
        }

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var activeAdminCount = await context.Users.CountAsync(
                candidate => candidate.Role == UserRole.Admin && candidate.IsActive,
                cancellationToken);
            if (activeAdminCount <= 1)
            {
                return Result<UpdateUserAccessCommandResponse>.Failure(
                    ["Son aktif Admin hesabının rolü değiştirilemez."]);
            }
        }

        string? organizationName = null;
        string? departmentName = null;
        string? jurisdictionName = null;

        if (request.Role == UserRole.Official)
        {
            var validatedAccess = await OfficialAccessValidation.ValidateAsync(
                context,
                request.OrganizationDepartmentId!.Value,
                request.OfficialJurisdictionLevel!.Value,
                request.OfficialJurisdictionId!.Value,
                cancellationToken);
            organizationName = validatedAccess.OrganizationName;
            departmentName = validatedAccess.DepartmentName;
            jurisdictionName = validatedAccess.JurisdictionName;
        }

        var currentJurisdictionId = user.OfficialProvinceId ??
            user.OfficialDistrictId ??
            user.OfficialNeighborhoodId;

        var accessChanged =
            user.Role != request.Role ||
            user.OrganizationDepartmentId != request.OrganizationDepartmentId ||
            user.OfficialJurisdictionLevel != request.OfficialJurisdictionLevel ||
            currentJurisdictionId != request.OfficialJurisdictionId;

        user.UpdateAccess(
            request.Role,
            request.OrganizationDepartmentId,
            request.OfficialJurisdictionLevel,
            request.OfficialJurisdictionId);

        if (accessChanged)
        {
            await refreshTokenRevocationService.MarkAllActiveAsRevokedAsync(
                user.Id,
                "User access changed",
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateUserAccessCommandResponse>.Success(
            new UpdateUserAccessCommandResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!,
                user.Role,
                user.OrganizationDepartmentId,
                organizationName,
                departmentName,
                user.OfficialJurisdictionLevel,
                request.Role == UserRole.Official ? request.OfficialJurisdictionId : null,
                jurisdictionName));
    }

}
