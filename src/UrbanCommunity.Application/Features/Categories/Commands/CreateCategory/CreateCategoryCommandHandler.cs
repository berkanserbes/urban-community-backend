using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryCommandResponse>>
{
    public async Task<Result<CreateCategoryCommandResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await context.ReportCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name == request.Name.Trim(), cancellationToken);

        if (nameExists)
        {
            throw new AlreadyExistsException($"Bu isimde bir kategori zaten mevcut: {request.Name}");
        }

        var category = new ReportCategory
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IconUrl = string.IsNullOrWhiteSpace(request.IconUrl) ? null : request.IconUrl.Trim()
        };

        context.ReportCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result<CreateCategoryCommandResponse>.Success(
            new CreateCategoryCommandResponse(category.Id, category.Name, category.Description, category.IconUrl));
    }
}
