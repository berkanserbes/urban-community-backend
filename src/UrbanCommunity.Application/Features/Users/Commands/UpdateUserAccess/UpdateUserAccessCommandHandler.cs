using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
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

        string? departmentName = null;
        string? jurisdictionName = null;

        if (request.Role == UserRole.Official)
        {
            departmentName = await context.Departments
                .AsNoTracking()
                .Where(department => department.Id == request.DepartmentId)
                .Select(department => department.Name)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(Department), request.DepartmentId!.Value);

            jurisdictionName = await ResolveJurisdictionNameAsync(
                request.OfficialJurisdictionLevel!.Value,
                request.OfficialJurisdictionId!.Value,
                cancellationToken);
        }

        var currentJurisdictionId = user.OfficialProvinceId ??
            user.OfficialDistrictId ??
            user.OfficialNeighborhoodId;

        var accessChanged =
            user.Role != request.Role ||
            user.DepartmentId != request.DepartmentId ||
            user.OfficialJurisdictionLevel != request.OfficialJurisdictionLevel ||
            currentJurisdictionId != request.OfficialJurisdictionId;

        user.UpdateAccess(
            request.Role,
            request.DepartmentId,
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
                user.DepartmentId,
                departmentName,
                user.OfficialJurisdictionLevel,
                request.Role == UserRole.Official ? request.OfficialJurisdictionId : null,
                jurisdictionName));
    }

    private async Task<string> ResolveJurisdictionNameAsync(
        OfficialJurisdictionLevel level,
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        var name = level switch
        {
            OfficialJurisdictionLevel.Province => await context.Provinces
                .AsNoTracking()
                .Where(province => province.Id == jurisdictionId)
                .Select(province => province.Name)
                .FirstOrDefaultAsync(cancellationToken),

            OfficialJurisdictionLevel.District => await context.Districts
                .AsNoTracking()
                .Where(district => district.Id == jurisdictionId)
                .Select(district => district.Name)
                .FirstOrDefaultAsync(cancellationToken),

            OfficialJurisdictionLevel.Neighborhood => await context.Neighborhoods
                .AsNoTracking()
                .Where(neighborhood => neighborhood.Id == jurisdictionId)
                .Select(neighborhood => neighborhood.Name)
                .FirstOrDefaultAsync(cancellationToken),

            _ => null
        };

        return name ?? throw new NotFoundException(level.ToString(), jurisdictionId);
    }
}
