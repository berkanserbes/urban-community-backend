using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Features.Reports;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.CreateReport;

public sealed class CreateReportCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    IFileService fileService)
    : IRequestHandler<CreateReportCommand, Result<CreateReportCommandResponse>>
{
    public async Task<Result<CreateReportCommandResponse>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var userExists = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

        if (!userExists)
            throw new NotFoundException(nameof(User), request.UserId);

        var neighborhoodExists = await context.Neighborhoods
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.NeighborhoodId, cancellationToken);

        if (!neighborhoodExists)
            throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId);

        var location = CreateLocation(request.Latitude, request.Longitude);

        var category = await ReportCategoryAssignmentResolver.ResolveInitialAsync(
            context,
            request.SuggestedCategoryId,
            null,
            cancellationToken);

        var report = new Report
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = location,
            CategoryId = category.Id,
            NeighborhoodId = request.NeighborhoodId,
            UserId = request.UserId,
            Priority = ReportPriority.Medium,
            Status = ReportStatus.AiReview
        };
        report.SetContentHash(
            ReportSimilarityFingerprint.Create(report.Title, report.Description));

        var uploadedUrls = new List<string>();

        try
        {
            if (request.Media is { Count: > 0 })
            {
                for (var i = 0; i < request.Media.Count; i++)
                {
                    var file = request.Media[i];
                    string url;

                    try
                    {
                        url = await fileService.UploadFileAsync(file, "reports", cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch
                    {
                        await DeleteFilesBestEffortAsync(uploadedUrls, cancellationToken);
                        return Result<CreateReportCommandResponse>.Failure(["Medya dosyası yüklenemedi. Lütfen tekrar deneyin."]);
                    }

                    if (string.IsNullOrWhiteSpace(url))
                    {
                        await DeleteFilesBestEffortAsync(uploadedUrls, cancellationToken);
                        return Result<CreateReportCommandResponse>.Failure(["Medya dosyası yüklenemedi. Lütfen tekrar deneyin."]);
                    }

                    uploadedUrls.Add(url);
                    report.Media.Add(new ReportMedia
                    {
                        ReportId = report.Id,
                        Url = url,
                        OriginalFileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSizeBytes = file.Length,
                        DisplayOrder = i,
                        MediaType = ResolveMediaType(file)
                    });
                }
            }

            context.Reports.Add(report);

            // Published through the transactional outbox: SaveChanges stores it with the data below.
            await publishEndpoint.Publish(new ReportAiReviewRequestedEvent
            {
                ReportId = report.Id,
                IsInitialCategorization = true,
                RequestedAt = DateTime.UtcNow
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFilesBestEffortAsync(uploadedUrls, cancellationToken);
            throw;
        }

        var media = report.Media
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new CreateReportMediaResponse(
                m.Id,
                m.Url,
                m.OriginalFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.DisplayOrder,
                m.MediaType))
            .ToList();

        return Result<CreateReportCommandResponse>.Success(
            new CreateReportCommandResponse(
                report.Id,
                report.Title,
                report.Status,
                report.Priority,
                report.Location?.Y,
                report.Location?.X,
                report.CategoryId,
                category.Name,
                category.AssignedByAi,
                category.AiConfidence,
                report.NeighborhoodId,
                media,
                report.CreatedAt));
    }

    private static MediaType ResolveMediaType(IFormFile file)
    {
        return file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? MediaType.Video
            : MediaType.Image;
    }

    private static Point? CreateLocation(double? latitude, double? longitude)
    {
        return TryGetCoordinate(latitude, -90, 90, out var validLatitude) &&
            TryGetCoordinate(longitude, -180, 180, out var validLongitude)
            ? new Point(validLongitude, validLatitude) { SRID = 4326 }
            : null;
    }

    private static bool TryGetCoordinate(double? value, double min, double max, out double coordinate)
    {
        coordinate = value.GetValueOrDefault();
        return value.HasValue &&
            double.IsFinite(coordinate) &&
            coordinate >= min &&
            coordinate <= max;
    }

    private async Task DeleteFilesBestEffortAsync(IEnumerable<string> urls, CancellationToken cancellationToken)
    {
        foreach (var url in urls)
        {
            try
            {
                await fileService.DeleteFileAsync(url, cancellationToken);
            }
            catch
            {
                // Storage cleanup must not hide the original create failure.
            }
        }
    }
}
