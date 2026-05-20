using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Auth.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<LoginUserCommand, Result<LoginUserCommandResponse>>
{
    public async Task<Result<LoginUserCommandResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user is null)
        {
            return Result<LoginUserCommandResponse>.Failure(["Geçersiz e-posta veya şifre."]);
        }

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return Result<LoginUserCommandResponse>.Failure(["Geçersiz e-posta veya şifre."]);
        }

        var accessToken = tokenService.GenerateToken(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES"), out var minutes)
                ? minutes : 10);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var oldTokens = await context.UserRefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        var clientIp = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        foreach (var old in oldTokens)
        {
            old.RevokedAt = DateTime.UtcNow;
            old.RevokedByIp = clientIp;
            old.RevokedReason = "Previous token revoked due to new login";
        }

        var refreshToken = new UserRefreshToken
        {
            UserId = user.Id,
            Token = tokenService.HashToken(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS"), out var days)
                    ? days : 14),
            CreatedByIp = clientIp
        };

        context.UserRefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result<LoginUserCommandResponse>.Success(new LoginUserCommandResponse(
            accessToken,
            accessTokenExpiresAt,
            refreshTokenValue,
            refreshToken.ExpiresAt,
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.NeighborhoodId));
    }
}
