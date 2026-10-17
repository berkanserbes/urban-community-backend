namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;

public enum ReportAssignmentFilter
{
    /// <summary>Reports without an assigned organization department.</summary>
    Unassigned = 1,

    /// <summary>Reports whose automatic assignment must be reviewed.</summary>
    NeedsReview = 2
}
