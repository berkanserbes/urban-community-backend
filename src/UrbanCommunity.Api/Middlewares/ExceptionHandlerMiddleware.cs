using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Api.Middlewares;

//TODO@berkanserbes: En iyi şekilde düzenlenecek. Loglamalar gelistirilecek.

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errors) = MapException(exception);
        var traceId = context.TraceIdentifier;

        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "[{TraceId}] Beklenmeyen sunucu hatası: {ExceptionType} - {Message}",
                traceId, exception.GetType().Name, exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "[{TraceId}] İstemci hatası: {ExceptionType} ({StatusCode}) - {Message}",
                traceId, exception.GetType().Name, statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(
            IsSuccess: false,
            Errors: errors,
            TraceId: traceId
        );

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (int StatusCode, string[] Errors) MapException(Exception exception)
    {
        return exception switch
        {
            // 400 - Geçersiz girdi
            InvalidInputException invalidInputEx => (
                (int)HttpStatusCode.BadRequest,
                [invalidInputEx.Message]),

            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                validationEx.Errors.Select(e => e.ErrorMessage).ToArray()),

            // 401 - Kimlik doğrulama hatası
            InvalidTokenException invalidTokenEx => (
                (int)HttpStatusCode.Unauthorized,
                [invalidTokenEx.Message]),

            UnauthorizedException unauthorizedEx => (
                (int)HttpStatusCode.Unauthorized,
                [unauthorizedEx.Message]),

            // 403 - Yetki hatası
            AccountDeactivatedException accountEx => (
                (int)HttpStatusCode.Forbidden,
                [accountEx.Message]),

            ForbiddenAccessException forbiddenEx => (
                (int)HttpStatusCode.Forbidden,
                [forbiddenEx.Message]),

            // 404 - Kayıt bulunamadı
            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                [notFoundEx.Message]),

            // 409 - Çakışma
            ConcurrencyException concurrencyEx => (
                (int)HttpStatusCode.Conflict,
                [concurrencyEx.Message]),

            AlreadyExistsException alreadyExistsEx => (
                (int)HttpStatusCode.Conflict,
                [alreadyExistsEx.Message]),

            // 413 - Dosya çok büyük
            FileTooLargeException fileTooLargeEx => (
                (int)HttpStatusCode.RequestEntityTooLarge,
                [fileTooLargeEx.Message]),

            // 415 - Desteklenmeyen medya türü
            InvalidFileTypeException invalidFileEx => (
                (int)HttpStatusCode.UnsupportedMediaType,
                [invalidFileEx.Message]),

            // 422 - Domain iş kuralı ihlali
            DomainException domainEx => (
                (int)HttpStatusCode.UnprocessableEntity,
                [domainEx.Message]),

            // 429 - Rate limit
            RateLimitExceededException rateLimitEx => (
                429,
                [rateLimitEx.Message]),

            // 499 - İstemci isteği iptal etti
            OperationCanceledException => (
                499,
                ["İstek iptal edildi."]),

            // 502 - Harici servis hatası
            ExternalServiceException => (
                (int)HttpStatusCode.BadGateway,
                ["Harici bir servis şu an kullanılamıyor. Lütfen daha sonra tekrar deneyin."]),

            // 500 - Beklenmeyen hata
            _ => (
                (int)HttpStatusCode.InternalServerError,
                ["Sunucu tarafında beklenmeyen bir hata oluştu."])
        };
    }
}

internal sealed record ErrorResponse(bool IsSuccess, string[] Errors, string TraceId);
