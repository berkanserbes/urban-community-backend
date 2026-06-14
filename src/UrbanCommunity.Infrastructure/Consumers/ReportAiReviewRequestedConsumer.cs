using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Features.Reports;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportAiReviewRequestedConsumer(
    IApplicationDbContext context,
    IReportCategorizationService categorizationService,
    IReportPriorityService priorityService,
    IReportSimilarityService similarityService)
    : IConsumer<ReportAiReviewRequestedEvent>
{
    private const int CandidateLimit = 20;
    private const int MatchLimit = 5;
    private const double SimilarityThreshold = 0.75;
    private static readonly TimeSpan CandidateAge = TimeSpan.FromDays(30);

    public async Task Consume(ConsumeContext<ReportAiReviewRequestedEvent> consumeContext)
    {
        var evt = consumeContext.Message;
        var cancellationToken = consumeContext.CancellationToken;

        var report = await context.Reports
            .Include(candidate => candidate.Neighborhood)
                .ThenInclude(neighborhood => neighborhood.DistrictNavigation)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == evt.ReportId,
                cancellationToken);

        if (report is null || report.Status != ReportStatus.AiReview)
            return;

        var category = await ReportCategoryAssignmentResolver.ResolveFromAiAsync(
            context,
            categorizationService,
            report.Title,
            report.Description,
            report.CategoryId,
            cancellationToken);

        report.CategoryId = category.Id;

        var candidates = await GetSimilarityCandidatesAsync(report, cancellationToken);
        var exactMatches = candidates
            .Where(candidate => candidate.IsExactContentMatch)
            .Select(candidate => new ReportSimilaritySuggestion(
                candidate.Id,
                1.0,
                "Başlık ve açıklama içeriği birebir eşleşiyor."))
            .ToList();
        var similaritySuggestions = await similarityService.FindSimilarReportsAsync(
            report.Title,
            report.Description,
            candidates.Where(candidate => !candidate.IsExactContentMatch).ToList(),
            cancellationToken);

        var existingMatches = await context.ReportSimilarityMatches
            .Where(match =>
                match.ReportId == report.Id ||
                match.SimilarReportId == report.Id)
            .ToListAsync(cancellationToken);
        context.ReportSimilarityMatches.RemoveRange(existingMatches);

        var acceptedMatches = exactMatches
            .Concat(similaritySuggestions?
                .Where(suggestion => suggestion.SimilarityScore >= SimilarityThreshold)
                ?? [])
            .GroupBy(suggestion => suggestion.ReportId)
            .Select(group => group.MaxBy(suggestion => suggestion.SimilarityScore)!)
            .OrderByDescending(suggestion => suggestion.SimilarityScore)
            .Take(MatchLimit)
            .ToList();

        foreach (var match in acceptedMatches)
        {
            context.ReportSimilarityMatches.Add(new ReportSimilarityMatch
            {
                ReportId = report.Id,
                SimilarReportId = match.ReportId,
                SimilarityScore = match.SimilarityScore,
                Reasoning = match.Reasoning
            });
        }

        var daysOpen = Math.Max(
            0,
            (int)(DateTime.UtcNow.Date - report.CreatedAt.Date).TotalDays);

        var prioritySuggestion = await priorityService.SuggestPriorityAsync(
            report.Title,
            report.Description,
            category.Name,
            similarReportCount: acceptedMatches.Count,
            daysOpen: daysOpen,
            cancellationToken: cancellationToken);

        report.UpdatePriority(
            prioritySuggestion?.Priority ?? ReportPriority.Medium);

        report.ChangeStatus(
            ReportStatus.PendingReview,
            null,
            BuildReviewNote(
                category.AssignedByAi,
                prioritySuggestion is not null,
                similaritySuggestions is not null,
                acceptedMatches.Count));

        await context.SaveChangesAsync(cancellationToken);

        if (!evt.IsInitialCategorization)
            return;

        await consumeContext.Publish(new ReportCreatedEvent
        {
            ReportId = report.Id,
            ReportTitle = report.Title,
            CreatedByUserId = report.UserId,
            ProvinceId = report.Neighborhood.DistrictNavigation.ProvinceId,
            DistrictId = report.Neighborhood.DistrictId,
            NeighborhoodId = report.NeighborhoodId,
            CategoryId = report.CategoryId,
            CategoryName = category.Name,
            CategoryAssignedByAi = category.AssignedByAi,
            AiCategoryConfidence = category.AiConfidence,
            Priority = report.Priority,
            PriorityAssignedByAi = prioritySuggestion is not null,
            AiPriorityScore = prioritySuggestion?.Score,
            CreatedAt = report.CreatedAt
        });
    }

    private async Task<IReadOnlyList<ReportSimilarityCandidate>> GetSimilarityCandidatesAsync(
        Report report,
        CancellationToken cancellationToken)
    {
        var oldestCreatedAt = DateTime.UtcNow.Subtract(CandidateAge);
        var activeStatuses = new[]
        {
            ReportStatus.PendingReview,
            ReportStatus.AiReview,
            ReportStatus.Confirmed,
            ReportStatus.InProgress
        };

        return await context.Reports
            .AsNoTracking()
            .Where(candidate =>
                candidate.Id != report.Id &&
                candidate.NeighborhoodId == report.NeighborhoodId &&
                candidate.CreatedAt >= oldestCreatedAt &&
                activeStatuses.Contains(candidate.Status))
            .OrderByDescending(candidate =>
                candidate.ContentHash == report.ContentHash)
            .ThenByDescending(candidate => candidate.CreatedAt)
            .Take(CandidateLimit)
            .Select(candidate => new ReportSimilarityCandidate(
                candidate.Id,
                candidate.Title,
                candidate.Description,
                candidate.CreatedAt,
                report.ContentHash != null &&
                candidate.ContentHash == report.ContentHash))
            .ToListAsync(cancellationToken);
    }

    private static string BuildReviewNote(
        bool categoryAssignedByAi,
        bool priorityAssignedByAi,
        bool similarityAnalysisCompleted,
        int similarReportCount)
    {
        var categoryNote = categoryAssignedByAi
            ? "AI kategori incelemesi tamamlandı."
            : "AI kategori önerisi kullanılamadı; mevcut kategori korundu.";

        var priorityNote = priorityAssignedByAi
            ? "AI öncelik skoru uygulandı."
            : "AI öncelik skoru kullanılamadı; varsayılan Medium önceliği uygulandı.";

        var similarityNote = similarityAnalysisCompleted
            ? $"Benzerlik analizi tamamlandı; {similarReportCount} benzer rapor bulundu."
            : "Benzerlik analizi kullanılamadı.";

        return $"{categoryNote} {priorityNote} {similarityNote}";
    }
}
