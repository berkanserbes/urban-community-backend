using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCategoryCommand, Result<DeleteCategoryCommandResponse>>
{
    public async Task<Result<DeleteCategoryCommandResponse>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.ReportCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("ReportCategory", request.Id);
        }

        var hasReports = await context.Reports
            .AsNoTracking()
            .AnyAsync(r => r.CategoryId == request.Id, cancellationToken);

        if (hasReports)
        {
            return Result<DeleteCategoryCommandResponse>.Failure(["Bu kategoriye ait raporlar bulunduğu için silinemez."]);
        }

        context.ReportCategories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteCategoryCommandResponse>.Success(new DeleteCategoryCommandResponse("Kategori başarıyla silindi."));
    }
}
