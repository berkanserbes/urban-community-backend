using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Reports;

public static class ReportCategoryAssignmentResolver
{
    private const double MinimumAiConfidence = 0.60;
    private const string FallbackCategoryName = "Diğer";

    public static async Task<ResolvedReportCategory> ResolveInitialAsync(
        IApplicationDbContext context,
        Guid? suggestedCategoryId,
        Guid? currentCategoryId,
        CancellationToken cancellationToken)
    {
        var preferredCategoryId = suggestedCategoryId ?? currentCategoryId;

        if (preferredCategoryId.HasValue)
        {
            var preferredCategory = await context.ReportCategories
                .AsNoTracking()
                .Where(category => category.Id == preferredCategoryId.Value)
                .Select(category => new ResolvedReportCategory(
                    category.Id,
                    category.Name,
                    false,
                    null))
                .FirstOrDefaultAsync(cancellationToken);

            if (preferredCategory is not null)
                return preferredCategory;

            throw new NotFoundException(nameof(ReportCategory), preferredCategoryId.Value);
        }

        return await GetFallbackCategoryAsync(context, cancellationToken);
    }

    public static async Task<ResolvedReportCategory> ResolveFromAiAsync(
        IApplicationDbContext context,
        IReportCategorizationService categorizationService,
        string title,
        string description,
        Guid fallbackCategoryId,
        CancellationToken cancellationToken)
    {
        var categories = await context.ReportCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new ReportCategoryCandidate(
                category.Id,
                category.Name,
                category.Description))
            .ToListAsync(cancellationToken);

        var aiSuggestion = await categorizationService.SuggestCategoryAsync(
            title.Trim(),
            description.Trim(),
            categories,
            cancellationToken);

        if (aiSuggestion is not null &&
            aiSuggestion.Confidence >= MinimumAiConfidence)
        {
            var matchedCategory = categories.FirstOrDefault(
                category => category.Id == aiSuggestion.CategoryId);

            if (matchedCategory is not null)
            {
                return new ResolvedReportCategory(
                    matchedCategory.Id,
                    matchedCategory.Name,
                    true,
                    aiSuggestion.Confidence);
            }
        }

        var fallbackCategory = categories.FirstOrDefault(
            category => category.Id == fallbackCategoryId)
            ?? categories.FirstOrDefault(category => category.Name == FallbackCategoryName);

        if (fallbackCategory is null)
            return await GetFallbackCategoryAsync(context, cancellationToken);

        return new ResolvedReportCategory(
            fallbackCategory.Id,
            fallbackCategory.Name,
            false,
            aiSuggestion?.Confidence);
    }

    private static async Task<ResolvedReportCategory> GetFallbackCategoryAsync(
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var fallbackCategory = await context.ReportCategories
            .AsNoTracking()
            .Where(category => category.Name == FallbackCategoryName)
            .Select(category => new ResolvedReportCategory(
                category.Id,
                category.Name,
                false,
                null))
            .FirstOrDefaultAsync(cancellationToken);

        if (fallbackCategory is not null)
            return fallbackCategory;

        var fallback = new ReportCategory
        {
            Name = FallbackCategoryName,
            Description = "AI tarafından güvenle sınıflandırılamayan raporlar."
        };

        context.ReportCategories.Add(fallback);
        await context.SaveChangesAsync(cancellationToken);

        return new ResolvedReportCategory(fallback.Id, fallback.Name, false, null);
    }
}

public sealed record ResolvedReportCategory(
    Guid Id,
    string Name,
    bool AssignedByAi,
    double? AiConfidence);
