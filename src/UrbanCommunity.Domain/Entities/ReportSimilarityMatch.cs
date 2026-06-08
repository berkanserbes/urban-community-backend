using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public sealed class ReportSimilarityMatch : BaseEntity
{
    public Guid ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public Guid SimilarReportId { get; set; }
    public Report SimilarReport { get; set; } = null!;

    public double SimilarityScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}
