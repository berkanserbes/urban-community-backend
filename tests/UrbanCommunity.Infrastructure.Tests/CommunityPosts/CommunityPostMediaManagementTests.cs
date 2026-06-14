using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.CommunityPosts;

public sealed class CommunityPostMediaManagementTests
{
    [Fact]
    public async Task Update_WithNullKeepMediaIds_KeepsAllExistingMedia()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(2);
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService();
        var handler = CreateHandler(context, fileService);

        var result = await handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                null,
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Media.Count);
        Assert.Empty(fileService.DeletedUrls);
    }

    [Fact]
    public async Task Update_WithEmptyKeepMediaIds_DeletesAllExistingMedia()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(2);
        var existingUrls = post.Media.Select(media => media.Url).ToArray();
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService();
        var handler = CreateHandler(context, fileService);

        var result = await handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                [],
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!.Media);
        Assert.Equal(existingUrls.Order(), fileService.DeletedUrls.Order());
        Assert.Empty(await context.CommunityPostMedias.ToListAsync());
    }

    [Fact]
    public async Task Update_WithSelectedKeepMediaId_KeepsOnlySelectedMedia()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(2);
        var keptMedia = post.Media.First();
        var removedMedia = post.Media.Last();
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService();
        var handler = CreateHandler(context, fileService);

        var result = await handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                [keptMedia.Id],
                null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(keptMedia.Id, Assert.Single(result.Data!.Media).Id);
        Assert.Equal(removedMedia.Url, Assert.Single(fileService.DeletedUrls));
    }

    [Fact]
    public async Task Update_WithForeignKeepMediaId_ReturnsFailureWithoutDeletingFiles()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(1);
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService();
        var handler = CreateHandler(context, fileService);

        var result = await handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                [Guid.NewGuid()],
                null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(fileService.DeletedUrls);
        Assert.Single(await context.CommunityPostMedias.ToListAsync());
    }

    [Fact]
    public async Task Update_WhenLaterUploadFails_DeletesEarlierUploadedFiles()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(0);
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService("/uploads/new-1.jpg", string.Empty);
        var handler = CreateHandler(context, fileService);

        var result = await handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                null,
                [CreateFile("one.jpg"), CreateFile("two.jpg")]),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("/uploads/new-1.jpg", Assert.Single(fileService.DeletedUrls));
        Assert.Empty(await context.CommunityPostMedias.ToListAsync());
    }

    [Fact]
    public async Task Update_WhenDatabaseSaveFails_DeletesNewlyUploadedFiles()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new FailingSaveDbContext(options);
        var post = CreatePostWithMedia(0);
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();
        context.FailOnSave = true;

        var fileService = new FakeFileService("/uploads/new.jpg");
        var handler = CreateHandler(context, fileService);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new UpdateCommunityPostCommand(
                post.Id,
                post.UserId,
                "Updated",
                "Updated content",
                null,
                [CreateFile("new.jpg")]),
            CancellationToken.None));

        Assert.Equal("/uploads/new.jpg", Assert.Single(fileService.DeletedUrls));
    }

    [Fact]
    public async Task Create_WhenLaterUploadFails_DeletesEarlierUploadedFiles()
    {
        await using var context = CreateContext();
        var location = await SeedLocationAsync(context);
        var fileService = new FakeFileService(
            "/uploads/created-1.jpg",
            string.Empty);
        var handler = new CreateCommunityPostCommandHandler(
            context,
            fileService,
            NullLogger<CreateCommunityPostCommandHandler>.Instance);

        var result = await handler.Handle(
            new CreateCommunityPostCommand(
                PostType.HelpRequested,
                "Post",
                "Content",
                location.User.Id,
                CommunityPostScope.Neighborhood,
                NeighborhoodId: location.Neighborhood.Id,
                Media: [CreateFile("one.jpg"), CreateFile("two.jpg")]),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "/uploads/created-1.jpg",
            Assert.Single(fileService.DeletedUrls));
        Assert.Empty(await context.CommunityPosts.ToListAsync());
    }

    [Fact]
    public async Task Delete_RemovesPhysicalMediaAfterDatabaseDelete()
    {
        await using var context = CreateContext();
        var post = CreatePostWithMedia(2);
        var urls = post.Media.Select(media => media.Url).ToArray();
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var fileService = new FakeFileService();
        var handler = new DeleteCommunityPostCommandHandler(
            context,
            fileService,
            NullLogger<DeleteCommunityPostCommandHandler>.Instance);

        var result = await handler.Handle(
            new DeleteCommunityPostCommand(post.Id, post.UserId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(urls.Order(), fileService.DeletedUrls.Order());
        Assert.Empty(await context.CommunityPosts.ToListAsync());
    }

    private static UpdateCommunityPostCommandHandler CreateHandler(
        ApplicationDbContext context,
        IFileService fileService)
    {
        return new UpdateCommunityPostCommandHandler(
            context,
            fileService,
            NullLogger<UpdateCommunityPostCommandHandler>.Instance);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CommunityPost CreatePostWithMedia(int mediaCount)
    {
        var post = new CommunityPost
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = PostType.HelpRequested,
            Title = "Post",
            Content = "Content",
            Status = CommunityPostStatus.Active,
            Scope = CommunityPostScope.Neighborhood,
            NeighborhoodId = Guid.NewGuid()
        };

        for (var index = 0; index < mediaCount; index++)
        {
            post.Media.Add(new CommunityPostMedia
            {
                Id = Guid.NewGuid(),
                CommunityPostId = post.Id,
                Url = $"/uploads/existing-{index}.jpg",
                DisplayOrder = index,
                MediaType = MediaType.Image
            });
        }

        return post;
    }

    private static IFormFile CreateFile(string name)
    {
        return new FormFile(
            new MemoryStream([1, 2, 3]),
            0,
            3,
            "file",
            name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private static async Task<LocationData> SeedLocationAsync(
        ApplicationDbContext context)
    {
        var province = new Province { Name = "Province" };
        var district = new District
        {
            Name = "District",
            Province = province
        };
        var neighborhood = new Neighborhood
        {
            Name = "Neighborhood",
            District = district.Name,
            City = province.Name,
            DistrictNavigation = district
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Post",
            LastName = "Owner",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            Neighborhood = neighborhood,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return new LocationData(user, neighborhood);
    }

    private sealed class FakeFileService(params string[] uploadResults) : IFileService
    {
        private readonly Queue<string> _uploadResults = new(uploadResults);

        public List<string> DeletedUrls { get; } = [];

        public Task<string> UploadFileAsync(
            IFormFile file,
            string folderName,
            CancellationToken cancellationToken = default)
        {
            var result = _uploadResults.Count > 0
                ? _uploadResults.Dequeue()
                : $"/uploads/{Guid.NewGuid():N}.jpg";
            return Task.FromResult(result);
        }

        public Task<bool> DeleteFileAsync(
            string fileUrl,
            CancellationToken cancellationToken = default)
        {
            DeletedUrls.Add(fileUrl);
            return Task.FromResult(true);
        }
    }

    private sealed class FailingSaveDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : ApplicationDbContext(options)
    {
        public bool FailOnSave { get; set; }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return FailOnSave
                ? throw new InvalidOperationException("Simulated database failure.")
                : base.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed record LocationData(User User, Neighborhood Neighborhood);
}
