using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UrbanCommunity.Api.Controllers;
using UrbanCommunity.Api.Extensions;
using UrbanCommunity.Api.Middlewares;
using UrbanCommunity.Application;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Pagination;

public sealed class PaginationApiTests
{
    private static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData("", 1, 20, 3)]
    [InlineData("?pageIndex=2&pageSize=2", 2, 2, 1)]
    [InlineData("?pageIndex=9&pageSize=2", 9, 2, 0)]
    public async Task HttpQueryBindingAndWrappedMetadata(string query, int page, int size, int itemCount)
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        await SeedAsync(server);
        using var client = server.CreateClient();
        using var response = await client.GetAsync("/api/users" + query);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        var data = json.RootElement.GetProperty("data");
        Assert.Equal(7, data.EnumerateObject().Count());
        Assert.Equal(page, data.GetProperty("pageIndex").GetInt32());
        Assert.Equal(size, data.GetProperty("pageSize").GetInt32());
        Assert.Equal(3, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(itemCount, data.GetProperty("items").GetArrayLength());
    }

    [Theory]
    [InlineData("pageIndex=0")]
    [InlineData("pageIndex=-1")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=-1")]
    [InlineData("pageSize=101")]
    [InlineData("pageIndex=2147483647&pageSize=100")]
    [InlineData("pageIndex=invalid")]
    [InlineData("pageSize=2147483648")]
    public async Task InvalidPaginationReturns400EvenWithoutAnEndpointValidator(string query)
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        using var client = server.CreateClient();
        using var response = await client.GetAsync("/api/users?" + query);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AllMigratedRoutesPreserveFiltersAndReturnPages()
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        var seed = await SeedAsync(server);
        using var client = server.CreateClient();
        var routes = new (string Route, int Count)[]
        {
            ("/api/reports", 3),
            ("/api/reports/mine", 3),
            ("/api/reports/manageable", 4),
            ("/api/suggestions?status=Active", 3),
            ($"/api/community-posts?neighborhoodId={seed.Neighborhood.Id}", 3),
            ("/api/announcements", 4),
            ("/api/announcements/manageable", 5),
            ("/api/notifications?isRead=false", 3),
            ("/api/users", 3),
            ($"/api/users/by-province/{seed.Province.Id}", 3),
            ($"/api/users/by-district/{seed.District.Id}", 3),
            ($"/api/users/by-neighborhood/{seed.Neighborhood.Id}", 2),
            ($"/api/provinces/{seed.Province.Id}/users", 3),
            ($"/api/districts/{seed.District.Id}/users", 3),
            ($"/api/neighborhoods/{seed.Neighborhood.Id}/users", 2),
            ($"/api/reports/{seed.Report.Id}/comments", 3),
            ($"/api/suggestions/{seed.Suggestion.Id}/comments", 3),
            ($"/api/community-posts/{seed.Post.Id}/comments", 3),
            ($"/api/suggestions/{seed.Suggestion.Id}/votes/voters?isUpvote=true", 2)
        };

        foreach (var (route, count) in routes)
        {
            var separator = route.Contains('?') ? '&' : '?';
            using var response = await client.GetAsync($"{route}{separator}pageIndex=2&pageSize=1");
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"{route}: {response.StatusCode}: {body}");
            var result = JsonSerializer.Deserialize<Result<PagedResult<JsonElement>>>(body, JsonOptions)!;
            Assert.Equal(count, result.Data!.TotalCount);
            Assert.Equal(count, result.Data.TotalPages);
            Assert.Equal(2, result.Data.PageIndex);
            Assert.Single(result.Data.Items);
        }

        using var votes = JsonDocument.Parse(await client.GetStringAsync(
            $"/api/suggestions/{seed.Suggestion.Id}/votes/voters?isUpvote=false"));
        var voter = Assert.Single(votes.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
        Assert.False(voter.GetProperty("isUpvote").GetBoolean());
        Assert.Equal("Outside User", voter.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task ServerSideFiltersCoverAllRecords()
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        await SeedAsync(server);
        using var client = server.CreateClient();

        foreach (var (route, count) in new (string Route, int Count)[]
        {
            ("/api/reports/manageable?search=ROAD", 4),
            ("/api/reports/manageable?search=outside", 1),
            ("/api/reports/manageable?priority=Urgent", 0),
            ("/api/reports/manageable?assignment=Unassigned", 4),
            ("/api/reports/manageable?assignment=NeedsReview", 0),
            ("/api/users?search=peer", 1),
            ("/api/users?role=Citizen", 2),
            ("/api/users?isActive=false", 0),
            ("/api/announcements/manageable?search=other", 1),
            ("/api/announcements/manageable?scope=All", 5),
            ("/api/announcements/manageable?isActive=false", 0)
        })
        {
            using var json = JsonDocument.Parse(await client.GetStringAsync(route));
            Assert.True(count == json.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32(), route);
        }
    }

    [Fact]
    public async Task TiedSortKeysYieldDistinctStablePagesAndProjectionKeepsNames()
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        var seed = await SeedAsync(server);
        using var client = server.CreateClient();
        var seen = new HashSet<Guid>();
        for (var page = 1; page <= 3; page++)
        {
            using var json = JsonDocument.Parse(await client.GetStringAsync(
                $"/api/reports/{seed.Report.Id}/comments?pageIndex={page}&pageSize=1"));
            var item = Assert.Single(json.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
            Assert.True(seen.Add(item.GetProperty("id").GetGuid()));
            Assert.Equal("Owner User", item.GetProperty("authorName").GetString());
        }
        Assert.Equal(3, seen.Count);
    }

    [Fact]
    public async Task CommentAuthorizationAndMissingParentsRemainEnforced()
    {
        using var host = await CreateHost(isAdmin: false);
        var server = host.GetTestServer();
        var seed = await SeedAsync(server);
        using var client = server.CreateClient();
        using var forbidden = await client.GetAsync($"/api/community-posts/{seed.OutsidePost.Id}/comments");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        foreach (var route in new[] { "reports", "suggestions", "community-posts" })
        {
            using var missing = await client.GetAsync($"/api/{route}/{Guid.NewGuid()}/comments");
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }
        using var posts = JsonDocument.Parse(await client.GetStringAsync("/api/community-posts?pageSize=1"));
        Assert.Equal(3, posts.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        // A client cannot bind the internal IsAdmin or RequestingUserId fields.
        using var injected = JsonDocument.Parse(await client.GetStringAsync(
            "/api/community-posts?pageSize=1&isAdmin=true&requestingUserId=" + Guid.NewGuid()));
        Assert.Equal(3, injected.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData("Owner", "User", "Owner User")]
    [InlineData(" ", "User", "User")]
    [InlineData("Owner", " ", "Owner")]
    [InlineData(" ", " ", "")]
    public async Task SqlReadyNameProjectionsKeepExistingFullNameSemantics(string first, string last, string expected)
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        var seed = await SeedAsync(server);
        using (var scope = server.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var owner = await context.Users.SingleAsync(user => user.Id == OwnerId);
            owner.FirstName = first;
            owner.LastName = last;
            await context.SaveChangesAsync();
        }
        using var client = server.CreateClient();
        foreach (var (route, field) in new[]
        {
            ($"/api/reports/{seed.Report.Id}/comments", "authorName"),
            ($"/api/suggestions/{seed.Suggestion.Id}/comments", "authorName"),
            ($"/api/community-posts/{seed.Post.Id}/comments", "authorName"),
            ("/api/announcements", "userFullName"),
            ("/api/announcements/manageable", "userFullName"),
            ("/api/reports", "reporterName"),
            ("/api/community-posts", "userFullName"),
            ("/api/suggestions", "userFullName"),
            ($"/api/suggestions/{seed.Suggestion.Id}/votes/voters", "fullName")
        })
        {
            using var json = JsonDocument.Parse(await client.GetStringAsync(route));
            var items = json.RootElement.GetProperty("data").GetProperty("items").EnumerateArray()
                .Where(item => item.GetProperty("userId").GetGuid() == OwnerId).ToArray();
            Assert.NotEmpty(items);
            Assert.All(items, item => Assert.Equal(expected, item.GetProperty(field).GetString()));
        }
    }

    [Fact]
    public async Task OpenApiDescribesFlatPaginationAndPagedResponse()
    {
        using var host = await CreateHost();
        var server = host.GetTestServer();
        using var client = server.CreateClient();
        using var json = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var operation = json.RootElement.GetProperty("paths").GetProperty("/api/users").GetProperty("get");
        var parameters = operation.GetProperty("parameters").EnumerateArray()
            .Select(parameter => parameter.GetProperty("name").GetString()).ToArray();
        Assert.Contains(parameters, name => string.Equals(name, "pageIndex", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(parameters, name => string.Equals(name, "pageSize", StringComparison.OrdinalIgnoreCase));
        var schema = operation.GetProperty("responses").GetProperty("200")
            .GetProperty("content").GetProperty("application/json").GetProperty("schema");
        Assert.Contains("PagedResult", schema.GetRawText());
        Assert.Contains("hasNextPage", json.RootElement.GetProperty("components").GetRawText());
    }

    private static Task<IHost> CreateHost(bool isAdmin = true)
    {
        var database = Guid.NewGuid().ToString();
        return new HostBuilder().ConfigureWebHost(web => web
            .UseTestServer()
            .UseSetting(WebHostDefaults.ApplicationKey, typeof(UsersController).Assembly.GetName().Name)
            .ConfigureServices(services =>
            {
                services.AddApiServices();
                services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);
                services.AddAuthorization();
                services.AddAuthentication();
                services.AddApplicationServices();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(database));
                services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
                services.AddScoped<IReportManagementAuthorizationService, ReportManagementAuthorizationService>();
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, InMemoryCacheService>();
            })
            .Configure(app =>
            {
                app.UseMiddleware<ExceptionHandlerMiddleware>();
                app.UseRouting();
                app.Use(async (context, next) =>
                {
                    context.User = new ClaimsPrincipal(new ClaimsIdentity([
                        new Claim(ClaimTypes.NameIdentifier, OwnerId.ToString()),
                        new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Citizen")
                    ], "Test"));
                    await next();
                });
                app.UseAuthorization();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); endpoints.MapOpenApi(); });
            })).StartAsync();
    }

    private static async Task<Seed> SeedAsync(TestServer server)
    {
        using var scope = server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var province = new Province { Name = "Province" };
        var district = new District { Name = "District", Province = province };
        var neighborhood = new Neighborhood { Name = "Main", DistrictNavigation = district, City = "Province", District = "District" };
        var outside = new Neighborhood { Name = "Outside", DistrictNavigation = district, City = "Province", District = "District" };
        var owner = new User { Id = OwnerId, FirstName = "Owner", LastName = "User", UserName = "owner", Email = "owner@example.com", IsActive = true, Role = UserRole.Admin, Neighborhood = neighborhood };
        var peer = new User { Id = Guid.NewGuid(), FirstName = "Peer", LastName = "User", UserName = "peer", Email = "peer@example.com", IsActive = true, Neighborhood = neighborhood };
        var other = new User { Id = Guid.NewGuid(), FirstName = "Outside", LastName = "User", UserName = "other", Email = "other@example.com", IsActive = true, Neighborhood = outside };
        context.AddRange(owner, peer, other);
        var category = new ReportCategory { Name = "Road" };
        Report? firstReport = null;
        Suggestion? firstSuggestion = null;
        CommunityPost? firstPost = null;
        var tiedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (var index = 0; index < 3; index++)
        {
            var report = new Report { Title = "Report", Description = "Description", User = owner, Category = category, Neighborhood = neighborhood };
            var suggestion = new Suggestion { Title = "Suggestion", Content = "Content", User = owner };
            var post = new CommunityPost { Title = "Post", Content = "Content", User = owner, Scope = CommunityPostScope.Neighborhood, Neighborhood = neighborhood };
            var announcement = new Announcement { Title = "News", Content = "Content", User = owner };
            foreach (var entity in new BaseEntity[] { report, suggestion, post, announcement }) entity.SetCreatedAt(tiedTime);
            context.AddRange(report, suggestion, post, announcement, new Notification { User = owner, Title = "Notification", Message = "Message" });
            firstReport ??= report;
            firstSuggestion ??= suggestion;
            firstPost ??= post;
        }
        var outsidePost = new CommunityPost { Title = "Other post", Content = "Content", User = other, Scope = CommunityPostScope.Neighborhood, Neighborhood = outside };
        context.AddRange(
            new Report { Title = "Other report", Description = "Description", User = other, Category = category, Neighborhood = outside },
            new Suggestion { Title = "Closed suggestion", Content = "Content", User = other, Status = SuggestionStatus.Closed },
            outsidePost,
            new Announcement { Title = "Other news", Content = "Content", User = peer },
            new Announcement { Title = "Expired news", Content = "Content", User = owner, ExpiresAt = DateTime.UtcNow.AddDays(-1) },
            new Notification { User = peer, Title = "Other notification", Message = "Message" },
            new Notification { User = owner, Title = "Read notification", Message = "Message", IsRead = true });
        for (var index = 0; index < 3; index++)
        {
            var reportComment = new ReportComment { Report = firstReport!, User = owner, Content = "Comment" };
            var suggestionComment = new SuggestionComment { Suggestion = firstSuggestion!, User = owner, Content = "Comment" };
            var postComment = new CommunityPostComment { CommunityPost = firstPost!, User = owner, Content = "Comment" };
            foreach (var entity in new BaseEntity[] { reportComment, suggestionComment, postComment }) entity.SetCreatedAt(tiedTime);
            context.AddRange(reportComment, suggestionComment, postComment);
        }
        context.Votes.AddRange(
            new Vote { Suggestion = firstSuggestion!, User = owner, IsUpvote = true },
            new Vote { Suggestion = firstSuggestion!, User = peer, IsUpvote = true },
            new Vote { Suggestion = firstSuggestion!, User = other, IsUpvote = false });
        await context.SaveChangesAsync();
        return new Seed(province, district, neighborhood, firstReport!, firstSuggestion!, firstPost!, outsidePost);
    }

    private sealed record Seed(Province Province, District District, Neighborhood Neighborhood,
        Report Report, Suggestion Suggestion, CommunityPost Post, CommunityPost OutsidePost);
}
