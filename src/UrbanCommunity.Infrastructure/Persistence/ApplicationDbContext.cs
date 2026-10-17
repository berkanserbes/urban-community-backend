using MassTransit;
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

    public override DbSet<User> Users => Set<User>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportCategory> ReportCategories => Set<ReportCategory>();
    public DbSet<ReportMedia> ReportMedias => Set<ReportMedia>();
    public DbSet<ReportHistory> ReportHistories => Set<ReportHistory>();
    public DbSet<ReportSimilarityMatch> ReportSimilarityMatches => Set<ReportSimilarityMatch>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<CommunityPostMedia> CommunityPostMedias => Set<CommunityPostMedia>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ReportComment> ReportComments => Set<ReportComment>();
    public DbSet<SuggestionComment> SuggestionComments => Set<SuggestionComment>();
    public DbSet<CommunityPostComment> CommunityPostComments => Set<CommunityPostComment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationDepartment> OrganizationDepartments => Set<OrganizationDepartment>();
    public DbSet<OrganizationDepartmentCategory> OrganizationDepartmentCategories =>
        Set<OrganizationDepartmentCategory>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // MassTransit transactional outbox: published messages are stored in the same
        // transaction as the business data, and the inbox drops redelivered messages.
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}
