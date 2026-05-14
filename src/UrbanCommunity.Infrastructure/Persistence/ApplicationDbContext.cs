using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Domain.Entities;

using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public override DbSet<User> Users => base.Users;
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportCategory> ReportCategories => Set<ReportCategory>();
    public DbSet<ReportMedia> ReportMedias => Set<ReportMedia>();
    public DbSet<ReportHistory> ReportHistories => Set<ReportHistory>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ReportComment> ReportComments => Set<ReportComment>();
    public DbSet<SuggestionComment> SuggestionComments => Set<SuggestionComment>();
    public DbSet<CommunityPostComment> CommunityPostComments => Set<CommunityPostComment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
