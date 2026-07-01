using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReportById;

public sealed class GetReportByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportByIdQuery, Result<GetReportByIdQueryResponse>>
{
    public async Task<Result<GetReportByIdQueryResponse>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Neighborhood)
            .Include(r => r.AssignedOrganizationDepartment)
                .ThenInclude(department => department!.Organization)
            .Include(r => r.AssignedOrganizationDepartment)
                .ThenInclude(department => department!.Department)
            .Include(r => r.Media.OrderBy(m => m.DisplayOrder))
            .Include(r => r.History.OrderByDescending(h => h.CreatedAt))
                .ThenInclude(h => h.ChangedByUser)
            .Include(r => r.SimilarReportMatches.OrderByDescending(match => match.SimilarityScore))
                .ThenInclude(match => match.SimilarReport)
            .Include(r => r.ReferencedAsSimilarMatches.OrderByDescending(match => match.SimilarityScore))
                .ThenInclude(match => match.Report)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.Id);

        var media = report.Media
            .Select(m => new ReportMediaDto(
                m.Id,
                m.Url,
                m.ThumbnailUrl,
                m.OriginalFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.DisplayOrder,
                m.MediaType))
            .ToList();

        var history = report.History
            .Select(h => new ReportHistoryDto(
                h.Id,
                h.FromStatus,
                h.ToStatus,
                h.Note,
                h.ChangedByUserId,
                h.ChangedByUser is null
                    ? null
                    : $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}".Trim(),
                h.CreatedAt))
            .ToList();

        var outgoingMatches = report.SimilarReportMatches
            .Select(match => new SimilarReportDto(
                match.SimilarReportId,
                match.SimilarReport.Title,
                match.SimilarReport.Status,
                match.SimilarityScore,
                match.Reasoning));

        var incomingMatches = report.ReferencedAsSimilarMatches
            .Select(match => new SimilarReportDto(
                match.ReportId,
                match.Report.Title,
                match.Report.Status,
                match.SimilarityScore,
                match.Reasoning));

        var similarReports = outgoingMatches
            .Concat(incomingMatches)
            .GroupBy(match => match.ReportId)
            .Select(group => group.MaxBy(match => match.SimilarityScore)!)
            .OrderByDescending(match => match.SimilarityScore)
            .ToList();

        var response = new GetReportByIdQueryResponse(
            report.Id,
            report.Title,
            report.Description,
            report.Status,
            report.Priority,
            report.Location?.Y,
            report.Location?.X,
            report.UserId,
            $"{report.User.FirstName} {report.User.LastName}",
            report.CategoryId,
            report.Category.Name,
            report.NeighborhoodId,
            report.Neighborhood.Name,
            report.AssignedOrganizationDepartmentId,
            report.AssignedOrganizationDepartment?.OrganizationId,
            report.AssignedOrganizationDepartment?.Organization.Name,
            report.AssignedOrganizationDepartment?.DepartmentId,
            report.AssignedOrganizationDepartment?.DisplayName ??
                report.AssignedOrganizationDepartment?.Department.Name,
            report.AssignedOrganizationDepartment?.ContactEmail,
            report.AssignedOrganizationDepartment?.ContactPhone,
            report.AssignmentNeedsReview,
            media,
            history,
            similarReports,
            report.CreatedAt,
            report.UpdatedAt,
            report.ResolvedAt);

        return Result<GetReportByIdQueryResponse>.Success(response);
    }
}
