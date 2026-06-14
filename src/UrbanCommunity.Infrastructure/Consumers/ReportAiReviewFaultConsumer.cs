using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportAiReviewFaultConsumer(
    IApplicationDbContext context)
    : IConsumer<Fault<ReportAiReviewRequestedEvent>>
{
    public async Task Consume(
        ConsumeContext<Fault<ReportAiReviewRequestedEvent>> consumeContext)
    {
        var evt = consumeContext.Message.Message;
        var cancellationToken = consumeContext.CancellationToken;

        var report = await context.Reports
            .Include(candidate => candidate.Neighborhood)
                .ThenInclude(neighborhood => neighborhood.DistrictNavigation)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == evt.ReportId,
                cancellationToken);

        if (report is null || report.Status != ReportStatus.AiReview)
            return;

        var categoryName = await context.ReportCategories
            .AsNoTracking()
            .Where(category => category.Id == report.CategoryId)
            .Select(category => category.Name)
            .FirstAsync(cancellationToken);

        report.UpdatePriority(ReportPriority.Medium);
        var existingMatches = await context.ReportSimilarityMatches
            .Where(match =>
                match.ReportId == report.Id ||
                match.SimilarReportId == report.Id)
            .ToListAsync(cancellationToken);
        context.ReportSimilarityMatches.RemoveRange(existingMatches);
        report.ChangeStatus(
            ReportStatus.PendingReview,
            null,
            "AI servisine geçici olarak ulaşılamadı; mevcut kategori ve varsayılan Medium önceliği uygulandı.");

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
            CategoryName = categoryName,
            CategoryAssignedByAi = false,
            AiCategoryConfidence = null,
            Priority = report.Priority,
            PriorityAssignedByAi = false,
            AiPriorityScore = null,
            CreatedAt = report.CreatedAt
        });
    }
}
