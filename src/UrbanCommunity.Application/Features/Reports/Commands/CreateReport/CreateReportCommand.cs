using MediatR;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.CreateReport;

public sealed record CreateReportCommand(
    string Title,
    string Description,
    double? Latitude,
    double? Longitude,
    Guid? SuggestedCategoryId,
    Guid NeighborhoodId,
    Guid UserId,
    IReadOnlyList<IFormFile>? Media = null) : IRequest<Result<CreateReportCommandResponse>>;
