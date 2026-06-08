using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Province> Provinces { get; }
    DbSet<District> Districts { get; }
    DbSet<Neighborhood> Neighborhoods { get; }
    DbSet<Report> Reports { get; }
    DbSet<ReportCategory> ReportCategories { get; }
    DbSet<ReportMedia> ReportMedias { get; }
    DbSet<ReportHistory> ReportHistories { get; }
    DbSet<ReportSimilarityMatch> ReportSimilarityMatches { get; }
    DbSet<Suggestion> Suggestions { get; }
    DbSet<Vote> Votes { get; }
    DbSet<CommunityPost> CommunityPosts { get; }
    DbSet<CommunityPostMedia> CommunityPostMedias { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ReportComment> ReportComments { get; }
    DbSet<SuggestionComment> SuggestionComments { get; }
    DbSet<CommunityPostComment> CommunityPostComments { get; }
    DbSet<Department> Departments { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
