using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Users;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.CreateOfficial;

public sealed class CreateOfficialCommandHandler(
    UserManager<User> userManager,
    IApplicationDbContext context,
    ITemporaryPasswordGenerator temporaryPasswordGenerator)
    : IRequestHandler<CreateOfficialCommand, Result<CreateOfficialCommandResponse>>
{
    public async Task<Result<CreateOfficialCommandResponse>> Handle(
        CreateOfficialCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
            throw new AlreadyExistsException("Bu e-posta adresiyle kayıtlı bir kullanıcı zaten mevcut.");

        var access = await OfficialAccessValidation.ValidateAsync(
            context,
            request.OrganizationDepartmentId,
            request.OfficialJurisdictionLevel,
            request.OfficialJurisdictionId,
            cancellationToken);

        var temporaryPassword = temporaryPasswordGenerator.Generate();
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            NeighborhoodId = null
        };
        user.UpdateAccess(
            UserRole.Official,
            request.OrganizationDepartmentId,
            request.OfficialJurisdictionLevel,
            request.OfficialJurisdictionId);
        user.RequirePasswordChange();

        var identityResult = await userManager.CreateAsync(user, temporaryPassword);
        if (!identityResult.Succeeded)
        {
            return Result<CreateOfficialCommandResponse>.Failure(
                identityResult.Errors.Select(error => error.Description));
        }

        return Result<CreateOfficialCommandResponse>.Success(new CreateOfficialCommandResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.Role,
            access.OrganizationDepartmentId,
            access.OrganizationName,
            access.DepartmentName,
            access.JurisdictionLevel,
            access.JurisdictionId,
            access.JurisdictionName,
            temporaryPassword,
            user.MustChangePassword));
    }
}
