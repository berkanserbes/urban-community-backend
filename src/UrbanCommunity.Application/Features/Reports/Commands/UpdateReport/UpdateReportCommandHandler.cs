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

namespace UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;

public sealed class UpdateReportCommandHandler(
    IApplicationDbContext context,
    IFileService fileService,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdateReportCommand, Result<UpdateReportCommandResponse>>
{
    private const int MaxMediaCount = 10;

    public async Task<Result<UpdateReportCommandResponse>> Handle(
        UpdateReportCommand request,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .Include(r => r.Media)
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        if (report.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu raporu yalnızca sahibi düzenleyebilir.");

        if (report.Status != ReportStatus.PendingReview)
            return Result<UpdateReportCommandResponse>.Failure(["Yalnızca beklemedeki raporlar düzenlenebilir."]);

        await EnsureNeighborhoodExistsAsync(request.NeighborhoodId, cancellationToken);

        var category = await ReportCategoryAssignmentResolver.ResolveInitialAsync(
            context,
            request.SuggestedCategoryId,
            report.CategoryId,
            cancellationToken);
        var assignmentInputsChanged =
            report.CategoryId != category.Id ||
            report.NeighborhoodId != request.NeighborhoodId;

        var mediaResult = await UpdateMediaAsync(
            report,
            request.DeleteMediaIds ?? [],
            request.Media ?? [],
            cancellationToken);

        if (!mediaResult.IsSuccess)
            return Result<UpdateReportCommandResponse>.Failure([mediaResult.Error!]);

        report.Title = request.Title.Trim();
        report.Description = request.Description.Trim();
        report.SetContentHash(
            ReportSimilarityFingerprint.Create(report.Title, report.Description));
        report.Location = CreateLocation(request.Latitude, request.Longitude);
        report.CategoryId = category.Id;
        report.NeighborhoodId = request.NeighborhoodId;
        if (assignmentInputsChanged && report.AssignedOrganizationDepartmentId.HasValue)
            report.MarkAssignmentForReview();
        var history = report.ChangeStatus(
            ReportStatus.AiReview,
            request.RequestingUserId,
            "Rapor güncellendi; AI incelemesine alındı.");

        if (history is not null)
            context.ReportHistories.Add(history);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFilesBestEffortAsync(mediaResult.UploadedUrls, cancellationToken);
            throw;
        }

        await DeleteFilesBestEffortAsync(mediaResult.DeletedUrls, cancellationToken);

        await publishEndpoint.Publish(new ReportAiReviewRequestedEvent
        {
            ReportId = report.Id,
            IsInitialCategorization = false,
            RequestedAt = DateTime.UtcNow
        }, cancellationToken);

        var media = report.Media
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new UpdateReportMediaResponse(
                m.Id,
                m.Url,
                m.OriginalFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.DisplayOrder,
                m.MediaType))
            .ToList();

        return Result<UpdateReportCommandResponse>.Success(
            new UpdateReportCommandResponse(
                report.Id,
                report.Title,
                report.Description,
                report.Location?.Y,
                report.Location?.X,
                report.Status,
                report.CategoryId,
                category.Name,
                category.AssignedByAi,
                category.AiConfidence,
                report.NeighborhoodId,
                report.Priority,
                media,
                report.UpdatedAt));
    }

    private async Task EnsureNeighborhoodExistsAsync(Guid neighborhoodId, CancellationToken cancellationToken)
    {
        if (!await context.Neighborhoods.AsNoTracking()
                .AnyAsync(n => n.Id == neighborhoodId, cancellationToken))
        {
            throw new NotFoundException(nameof(Neighborhood), neighborhoodId);
        }
    }

    private async Task<UpdateMediaResult> UpdateMediaAsync(
        Report report,
        IReadOnlyList<Guid> deleteMediaIds,
        IReadOnlyList<IFormFile> newMedia,
        CancellationToken cancellationToken)
    {
        var existingMedia = report.Media.ToList();

        if (deleteMediaIds.Any(id => existingMedia.All(media => media.Id != id)))
            return UpdateMediaResult.Failure("Silinmek istenen medya bu rapora ait değil.");

        var mediaToDelete = existingMedia
            .Where(media => deleteMediaIds.Contains(media.Id))
            .ToList();

        var keptMedia = existingMedia
            .Where(media => !deleteMediaIds.Contains(media.Id))
            .OrderBy(media => media.DisplayOrder)
            .ToList();

        if (keptMedia.Count + newMedia.Count > MaxMediaCount)
            return UpdateMediaResult.Failure($"Bir raporda en fazla {MaxMediaCount} medya dosyası bulunabilir.");

        for (var i = 0; i < keptMedia.Count; i++)
            keptMedia[i].DisplayOrder = i;

        var uploadedUrls = new List<string>();
        var uploadedMedia = new List<ReportMedia>();

        try
        {
            for (var i = 0; i < newMedia.Count; i++)
            {
                var file = newMedia[i];
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
                    return UpdateMediaResult.Failure("Medya dosyası yüklenemedi. Lütfen tekrar deneyin.");
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    await DeleteFilesBestEffortAsync(uploadedUrls, cancellationToken);
                    return UpdateMediaResult.Failure("Medya dosyası yüklenemedi. Lütfen tekrar deneyin.");
                }

                uploadedUrls.Add(url);
                uploadedMedia.Add(new ReportMedia
                {
                    ReportId = report.Id,
                    Url = url,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    DisplayOrder = keptMedia.Count + i,
                    MediaType = ResolveMediaType(file)
                });
            }
        }
        catch
        {
            await DeleteFilesBestEffortAsync(uploadedUrls, cancellationToken);
            throw;
        }

        foreach (var media in mediaToDelete)
        {
            context.ReportMedias.Remove(media);
            report.Media.Remove(media);
        }

        foreach (var media in uploadedMedia)
            report.Media.Add(media);

        return UpdateMediaResult.Success(
            mediaToDelete.Select(media => media.Url).ToList(),
            uploadedUrls);
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
                // Storage cleanup failure must not invalidate a completed database operation.
            }
        }
    }

    private static MediaType ResolveMediaType(IFormFile file) =>
        file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? MediaType.Video
            : MediaType.Image;

    private static Point? CreateLocation(double? latitude, double? longitude) =>
        TryGetCoordinate(latitude, -90, 90, out var validLatitude) &&
        TryGetCoordinate(longitude, -180, 180, out var validLongitude)
            ? new Point(validLongitude, validLatitude) { SRID = 4326 }
            : null;

    private static bool TryGetCoordinate(double? value, double min, double max, out double coordinate)
    {
        coordinate = value.GetValueOrDefault();
        return value.HasValue &&
            double.IsFinite(coordinate) &&
            coordinate >= min &&
            coordinate <= max;
    }

    private sealed record UpdateMediaResult(
        bool IsSuccess,
        string? Error,
        IReadOnlyList<string> DeletedUrls,
        IReadOnlyList<string> UploadedUrls)
    {
        public static UpdateMediaResult Success(
            IReadOnlyList<string> deletedUrls,
            IReadOnlyList<string> uploadedUrls) =>
            new(true, null, deletedUrls, uploadedUrls);

        public static UpdateMediaResult Failure(string error) =>
            new(false, error, [], []);
    }
}
