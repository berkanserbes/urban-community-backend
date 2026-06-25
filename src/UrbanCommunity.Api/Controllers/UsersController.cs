using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Application.Features.Users.Commands.ActivateUser;
using UrbanCommunity.Application.Features.Users.Commands.AdminUpdateUser;
using UrbanCommunity.Application.Features.Users.Commands.ChangePassword;
using UrbanCommunity.Application.Features.Users.Commands.CreateOfficial;
using UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;
using UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;
using UrbanCommunity.Application.Features.Users.Commands.UpdateProfile;
using UrbanCommunity.Application.Features.Users.Commands.UpdateProfileImage;
using UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;
using UrbanCommunity.Application.Features.Users.Commands.RequiredPasswordChange;
using UrbanCommunity.Application.Features.Users.Queries.GetCurrentUser;
using UrbanCommunity.Application.Features.Users.Queries.GetUserById;
using UrbanCommunity.Application.Features.Users.Queries.GetUsers;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByDistrict;
using UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator) : ApiController
{
    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetCurrentUserQuery(userId));
        return Ok(result);
    }

    /// <summary>Updates the display name, bio, phone number, and neighborhood of the authenticated user.</summary>
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateProfileCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.Bio,
            request.NeighborhoodId,
            request.PhoneNumber));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Uploads or replaces the profile image of the authenticated user (multipart/form-data).</summary>
    [HttpPost("me/profile-image")]
    [RequestSizeLimit(ApiConstants.MaxUploadSize)]
    public async Task<IActionResult> UpdateProfileImage(IFormFile image)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateProfileImageCommand(userId, image));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Removes the profile image of the authenticated user.</summary>
    [HttpDelete("me/profile-image")]
    public async Task<IActionResult> DeleteProfileImage()
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new DeleteProfileImageCommand(userId));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Changes the password of the authenticated user after verifying the current password.</summary>
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("me/required-password")]
    public async Task<IActionResult> ChangeRequiredPassword(
        [FromBody] RequiredPasswordChangeRequest request)
    {
        var result = await mediator.Send(new RequiredPasswordChangeCommand(
            GetCurrentUserId(),
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Returns the public profile of the specified user.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }

    /// <summary>Returns all registered users. Restricted to Admin.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    [HttpPost("officials")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOfficial([FromBody] CreateOfficialCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Returns active users filtered by province. Restricted to Admin.</summary>
    [HttpGet("by-province/{provinceId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByProvince(Guid provinceId)
    {
        var result = await mediator.Send(new GetUsersByProvinceQuery(provinceId));
        return Ok(result);
    }

    /// <summary>Returns active users filtered by district. Restricted to Admin.</summary>
    [HttpGet("by-district/{districtId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByDistrict(Guid districtId)
    {
        var result = await mediator.Send(new GetUsersByDistrictQuery(districtId));
        return Ok(result);
    }

    /// <summary>Returns active users filtered by neighborhood. Restricted to Admin.</summary>
    [HttpGet("by-neighborhood/{neighborhoodId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByNeighborhood(Guid neighborhoodId)
    {
        var result = await mediator.Send(new GetUsersByNeighborhoodQuery(neighborhoodId));
        return Ok(result);
    }

    /// <summary>Updates a user's profile information. Restricted to Admin.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminUpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request)
    {
        var result = await mediator.Send(new AdminUpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Bio,
            request.PhoneNumber,
            request.NeighborhoodId));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Updates a user's role, department, and Official jurisdiction. Restricted to Admin.</summary>
    [HttpPut("{id:guid}/access")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccess(Guid id, [FromBody] UpdateUserAccessRequest request)
    {
        var result = await mediator.Send(new UpdateUserAccessCommand(
            id,
            request.Role,
            request.OrganizationDepartmentId,
            request.OfficialJurisdictionLevel,
            request.OfficialJurisdictionId,
            GetCurrentUserId()));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Deactivates the specified user account. Restricted to Admin.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await mediator.Send(new DeactivateUserCommand(id, GetCurrentUserId()));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>Reactivates a previously deactivated user account. Restricted to Admin.</summary>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await mediator.Send(new ActivateUserCommand(id));
        return Ok(result);
    }

}

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Bio,
    Guid? NeighborhoodId,
    string? PhoneNumber);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record RequiredPasswordChangeRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record AdminUpdateUserRequest(
    string FirstName,
    string LastName,
    string? Bio,
    string? PhoneNumber,
    Guid? NeighborhoodId);

public sealed record UpdateUserAccessRequest(
    UserRole Role,
    Guid? OrganizationDepartmentId,
    OfficialJurisdictionLevel? OfficialJurisdictionLevel,
    Guid? OfficialJurisdictionId);
