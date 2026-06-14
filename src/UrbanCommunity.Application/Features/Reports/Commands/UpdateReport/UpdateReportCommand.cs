using MediatR;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;

public sealed record UpdateReportCommand(
    Guid ReportId,
    Guid RequestingUserId,
    string Title,
    string Description,
    double? Latitude,
    double? Longitude,
    Guid? SuggestedCategoryId,
    Guid NeighborhoodId,
    IReadOnlyList<Guid>? DeleteMediaIds = null,
    IReadOnlyList<IFormFile>? Media = null) : IRequest<Result<UpdateReportCommandResponse>>;
