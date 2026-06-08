using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.DeleteReport;

public sealed class DeleteReportCommandHandler(
    IApplicationDbContext context,
    IFileService fileService)
    : IRequestHandler<DeleteReportCommand, Result<DeleteReportCommandResponse>>
{
    public async Task<Result<DeleteReportCommandResponse>> Handle(
        DeleteReportCommand request,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .Include(r => r.Media)
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        if (!request.IsAdmin && report.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu raporu silme yetkiniz yok.");

        if (!request.IsAdmin && report.Status != ReportStatus.PendingReview)
            return Result<DeleteReportCommandResponse>.Failure(["Yalnızca beklemedeki raporlar silinebilir."]);

        var mediaUrls = report.Media.Select(media => media.Url).ToList();

        context.Reports.Remove(report);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var url in mediaUrls)
        {
            try
            {
                await fileService.DeleteFileAsync(url, cancellationToken);
            }
            catch
            {
                // The report is already deleted; storage cleanup can be retried separately.
            }
        }

        return Result<DeleteReportCommandResponse>.Success(
            new DeleteReportCommandResponse("Rapor başarıyla silindi."));
    }
}
