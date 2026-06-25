using System.Security.Claims;
using System.Text.Json;

namespace UrbanCommunity.Api.Middlewares;

public sealed class RequiredPasswordChangeMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        var mustChangePassword = context.User.Identity?.IsAuthenticated == true &&
            string.Equals(
                context.User.FindFirstValue("must_change_password"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        if (mustChangePassword && !IsAllowedRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                isSuccess = false,
                code = "PASSWORD_CHANGE_REQUIRED",
                errors = new[]
                {
                    "Devam etmek için geçici şifrenizi değiştirmeniz gerekiyor."
                }
            }, JsonOptions));
            return;
        }

        await next(context);
    }

    private static bool IsAllowedRequest(HttpRequest request)
    {
        if (request.Path.Equals("/api/users/me/required-password", StringComparison.OrdinalIgnoreCase))
            return true;

        if (request.Path.Equals("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
            return true;

        return request.Method == HttpMethods.Get &&
               request.Path.Equals("/api/users/me", StringComparison.OrdinalIgnoreCase);
    }
}
