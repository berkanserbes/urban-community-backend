using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Commands.DeleteAllCategories;

public sealed class DeleteAllCategoriesCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAllCategoriesCommand, Result<DeleteAllCategoriesCommandResponse>>
{
    public async Task<Result<DeleteAllCategoriesCommandResponse>> Handle(DeleteAllCategoriesCommand request, CancellationToken cancellationToken)
    {
        var categories = await context.ReportCategories
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            return Result<DeleteAllCategoriesCommandResponse>.Failure(["Silinecek kategori bulunamadı."]);
        }

        var categoriesWithReports = await context.ReportCategories
            .Where(c => context.Reports.AsNoTracking().Any(r => r.CategoryId == c.Id))
            .ToListAsync(cancellationToken);

        if (categoriesWithReports.Count > 0)
        {
            var categoryNames = string.Join(", ", categoriesWithReports.Select(c => c.Name));
            return Result<DeleteAllCategoriesCommandResponse>.Failure(
                [$"Bu kategorilere ait raporlar bulunduğu için silinemez: {categoryNames}"]);
        }

        context.ReportCategories.RemoveRange(categories);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAllCategoriesCommandResponse>.Success(
            new DeleteAllCategoriesCommandResponse(
                "Tüm kategoriler başarıyla silindi.",
                categories.Count));
    }
}
