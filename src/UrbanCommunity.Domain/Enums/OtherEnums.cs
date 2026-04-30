namespace UrbanCommunity.Domain.Enums;

public enum SuggestionStatus
{
    Active = 1,
    Reviewing = 2,
    Implementation = 3,
    Closed = 4
}

public enum PostType
{
    HelpRequested = 1,
    BorrowTool = 2,
    LostAndFound = 3
}

public enum CommunityPostStatus
{
    Active = 1,
    Reserved = 2,
    Resolved = 3,
    Expired = 4,
    Archived = 5
}

public enum MediaType
{
    Image = 1,
    Video = 2
}

public enum NotificationRelatedEntityType
{
    Report = 1,
    Suggestion = 2,
    CommunityPost = 3,
    Announcement = 4
}
