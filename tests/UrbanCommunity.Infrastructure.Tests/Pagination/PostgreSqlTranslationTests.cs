using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncements;
using UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncementStats;
using UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;
using UrbanCommunity.Application.Features.Reports.Queries.GetManageableReportStats;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionStats;
using UrbanCommunity.Application.Features.Users.Queries.GetUsers;
using UrbanCommunity.Application.Features.Users.Queries.GetUserStats;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Pagination;

/// <summary>
/// Runs the real handlers against the Npgsql provider without a database: the connection
/// is never opened and every command returns an empty result. A query that PostgreSQL
/// cannot translate fails before any command is recorded.
/// </summary>
public sealed class PostgreSqlTranslationTests
{
    private static readonly ReportManagementAccess OfficialAccess = new(
        false, Guid.NewGuid(), Guid.NewGuid(), OfficialJurisdictionLevel.District, Guid.NewGuid());

    [Fact]
    public async Task StatsQueriesTranslateToSingleAggregateCommand()
    {
        var commands = new CommandRecorder();
        await using var context = CreateContext(commands);

        await new GetManageableReportStatsQueryHandler(context, new FixedAccessService(OfficialAccess))
            .Handle(new GetManageableReportStatsQuery(OfficialAccess.RequestingUserId, false), CancellationToken.None);
        await new GetUserStatsQueryHandler(context).Handle(new GetUserStatsQuery(), CancellationToken.None);
        await new GetSuggestionStatsQueryHandler(context).Handle(new GetSuggestionStatsQuery(), CancellationToken.None);
        await new GetManageableAnnouncementStatsQueryHandler(context)
            .Handle(new GetManageableAnnouncementStatsQuery(Guid.NewGuid(), false), CancellationToken.None);

        Assert.Equal(4, commands.Commands.Count);
        Assert.All(commands.Commands, sql => Assert.Contains("count(", sql, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListFiltersTranslateToSql()
    {
        var commands = new CommandRecorder();
        await using var context = CreateContext(commands);

        await new GetManageableReportsQueryHandler(context, new FixedAccessService(OfficialAccess)).Handle(
            new GetManageableReportsQuery(
                OfficialAccess.RequestingUserId,
                false,
                Search: "yol",
                Priority: ReportPriority.Urgent,
                Assignment: ReportAssignmentFilter.NeedsReview),
            CancellationToken.None);
        await new GetUsersQueryHandler(context).Handle(
            new GetUsersQuery("ali", UserRole.Official, true), CancellationToken.None);
        await new GetManageableAnnouncementsQueryHandler(context).Handle(
            new GetManageableAnnouncementsQuery(Guid.NewGuid(), false, "su", AnnouncementScope.All, true),
            CancellationToken.None);

        // Each paged list issues one count and one page command.
        Assert.Equal(6, commands.Commands.Count);
        Assert.All(commands.Commands, sql => Assert.Contains("lower(", sql, StringComparison.OrdinalIgnoreCase));
    }

    private static ApplicationDbContext CreateContext(CommandRecorder commands)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=translation.invalid;Database=none", npgsql => npgsql.UseNetTopologySuite())
            .AddInterceptors(new SkipConnection(), commands)
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class FixedAccessService(ReportManagementAccess access) : IReportManagementAuthorizationService
    {
        private readonly ReportManagementAuthorizationService scope = new(null!);

        public Task<ReportManagementAccess> GetAccessAsync(Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken)
            => Task.FromResult(access);

        public IQueryable<Domain.Entities.Report> ApplyManageableScope(
            IQueryable<Domain.Entities.Report> reports,
            ReportManagementAccess reportAccess)
            => scope.ApplyManageableScope(reports, reportAccess);

        public Task EnsureCanManageAsync(Guid reportId, Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class SkipConnection : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(InterceptionResult.Suppress());
    }

    private sealed class CommandRecorder : DbCommandInterceptor
    {
        private static readonly Regex PagedCountCommand = new(
            @"^\s*SELECT count\(\*\)(::int)?\s+FROM", RegexOptions.IgnoreCase);

        public List<string> Commands { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);

            // Paged lists read a single-column count row first; everything else gets no rows.
            var table = new DataTable();
            if (PagedCountCommand.IsMatch(command.CommandText))
            {
                table.Columns.Add("count", typeof(int));
                table.Rows.Add(0);
            }

            return ValueTask.FromResult(InterceptionResult<DbDataReader>.SuppressWithResult(table.CreateDataReader()));
        }
    }
}
