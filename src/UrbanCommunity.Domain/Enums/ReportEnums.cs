namespace UrbanCommunity.Domain.Enums;

public enum ReportStatus
{
    Pending = 1,
    AI_Review = 2,
    Confirmed = 3,
    InProgress = 4,
    Resolved = 5,
    Rejected = 6
}

public enum ReportPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}
