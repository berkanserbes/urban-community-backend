using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryCommandResponse>>
{
    public async Task<Result<UpdateCategoryCommandResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.ReportCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("ReportCategory", request.Id);
        }

        var nameConflict = await context.ReportCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name == request.Name.Trim() && c.Id != request.Id, cancellationToken);

        if (nameConflict)
        {
            throw new AlreadyExistsException($"Bu isimde bir kategori zaten mevcut: {request.Name}");
        }

        category.Name = request.Name.Trim();
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.IconUrl = string.IsNullOrWhiteSpace(request.IconUrl) ? null : request.IconUrl.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateCategoryCommandResponse>.Success(
            new UpdateCategoryCommandResponse(category.Id, category.Name, category.Description, category.IconUrl));
    }
}
