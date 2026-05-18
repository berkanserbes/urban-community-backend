using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Categories.Commands.DeleteAllCategories;

public sealed record DeleteAllCategoriesCommand : IRequest<Result<DeleteAllCategoriesCommandResponse>>;
