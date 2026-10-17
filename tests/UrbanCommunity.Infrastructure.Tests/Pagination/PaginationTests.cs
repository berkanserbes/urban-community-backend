using System.Data.Common;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UrbanCommunity.Application.Behaviors;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Validators;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Pagination;

public sealed class PaginationTests
{
    [Theory]
    [InlineData(1, 20, 125, 7, false, true)]
    [InlineData(4, 20, 125, 7, true, true)]
    [InlineData(7, 20, 125, 7, true, false)]
    [InlineData(2, 20, 40, 2, true, false)]
    [InlineData(1, 20, 0, 0, false, false)]
    [InlineData(9, 20, 0, 0, true, false)]
    [InlineData(9, 20, 125, 7, true, false)]
    [InlineData(1, 1, int.MaxValue, int.MaxValue, false, true)]
    public void Metadata(int page, int size, int count, int pages, bool previous, bool next)
    {
        var result = new PagedResult<int>([], page, size, count);
        Assert.Equal(pages, result.TotalPages);
        Assert.Equal(previous, result.HasPreviousPage);
        Assert.Equal(next, result.HasNextPage);
        Assert.Equal(page, result.PageIndex);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task InvalidRequestIsRejectedBeforeQueryExecution(int page, int size)
    {
        // A LINQ-to-objects provider cannot execute EF async queries. The guards must win.
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new[] { 1 }.AsQueryable()
            .ToPagedResultAsync(page, size));
    }

    [Fact]
    public void DefaultsAndMaximumSizeAreValid()
    {
        var validator = new PaginationRequestValidator();
        var request = new PaginationRequest();
        Assert.Equal(1, request.PageIndex);
        Assert.Equal(20, request.PageSize);
        Assert.True(validator.Validate(request).IsValid);
        Assert.True(validator.Validate(request with { PageSize = 100 }).IsValid);
        Assert.True(validator.Validate(request with { PageIndex = int.MaxValue, PageSize = 1 }).IsValid);
    }

    [Fact]
    public async Task PipelineValidatesEvenWithoutAnEndpointValidator()
    {
        var behavior = new ValidationBehavior<TestQuery, int>([], new PaginationRequestValidator());
        var executed = false;
        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestQuery { PageIndex = 0 },
            _ => { executed = true; return Task.FromResult(1); }, CancellationToken.None));
        Assert.False(executed);
    }

    [Fact]
    public async Task PipelineAggregatesPaginationAndEndpointFailures()
    {
        var validator = new InlineValidator<TestQuery>();
        validator.RuleFor(query => query.Filter).NotEmpty();
        var behavior = new ValidationBehavior<TestQuery, int>([validator], new PaginationRequestValidator());
        var error = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestQuery { PageSize = 101 }, _ => Task.FromResult(1), CancellationToken.None));
        Assert.Contains(error.Errors, failure => failure.PropertyName == "PageSize");
        Assert.Contains(error.Errors, failure => failure.PropertyName == "Filter");
    }

    [Fact]
    public void WrappedResponseRoundTripsWithCamelCaseMetadata()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = Result<PagedResult<int>>.Success(new PagedResult<int>([21, 22], 2, 20, 22));
        var json = JsonSerializer.Serialize(response, options);
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(7, data.EnumerateObject().Count());
        Assert.Equal(22, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, data.GetProperty("items").GetArrayLength());
        var restored = JsonSerializer.Deserialize<Result<PagedResult<int>>>(json, options)!;
        Assert.True(restored.IsSuccess);
        Assert.Equal(response.Data!.Items, restored.Data!.Items);
        Assert.True(restored.Data.HasPreviousPage);
        Assert.False(restored.Data.HasNextPage);
    }

    [Fact]
    public async Task RelationalQueryCountsFilteredRowsAndFetchesOnlyTheRequestedPage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var commands = new CommandRecorder();
        var options = new DbContextOptionsBuilder<PaginationDbContext>()
            .UseSqlite(connection).AddInterceptors(commands).Options;
        await using var context = new PaginationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        context.Rows.AddRange(Enumerable.Range(1, 126).Select(id => new Row { Id = id, Group = 1 }));
        context.Rows.Add(new Row { Id = 127, Group = 2 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        commands.Commands.Clear();
        commands.Tokens.Clear();

        var query = context.Rows.AsNoTracking().Where(row => row.Group == 1 && row.Id <= 125)
            .OrderBy(row => row.Group).ThenBy(row => row.Id)
            .Select(row => new RowDto(row.Id));
        using var requestCancellation = new CancellationTokenSource();
        var result = await query.ToPagedResultAsync(2, 20, requestCancellation.Token);

        Assert.Equal(125, result.TotalCount);
        Assert.Equal(7, result.TotalPages);
        Assert.Equal(Enumerable.Range(21, 20), result.Items.Select(row => row.Id));
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(2, commands.Commands.Count);
        Assert.Contains("COUNT(*)", commands.Commands[0]);
        Assert.DoesNotContain("LIMIT", commands.Commands[0]);
        Assert.Contains("LIMIT", commands.Commands[1]);
        Assert.Contains("OFFSET", commands.Commands[1]);
        Assert.Contains("ORDER BY", commands.Commands[1]);
        Assert.All(commands.Tokens, token => Assert.Equal(requestCancellation.Token, token));

        var last = await query.ToPagedResultAsync(7, 20);
        Assert.Equal(5, last.Items.Count);
        var beyond = await query.ToPagedResultAsync(8, 20);
        Assert.Empty(beyond.Items);
        Assert.Equal(125, beyond.TotalCount);
        var empty = await context.Rows.Where(row => row.Id < 0).OrderBy(row => row.Id)
            .Select(row => new RowDto(row.Id)).ToPagedResultAsync(1, 20);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.TotalPages);

        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        commands.Commands.Clear();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => query.ToPagedResultAsync(1, 20, canceled.Token));
        Assert.Empty(commands.Commands);
    }

    private sealed record TestQuery : PaginationRequest, IRequest<int>
    {
        public string? Filter { get; init; }
    }

    private sealed record RowDto(int Id);
    private sealed class Row
    {
        public int Id { get; set; }
        public int Group { get; set; }
    }

    private sealed class PaginationDbContext(DbContextOptions<PaginationDbContext> options) : DbContext(options)
    {
        public DbSet<Row> Rows => Set<Row>();
    }

    private sealed class CommandRecorder : DbCommandInterceptor
    {
        public List<string> Commands { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);
            Tokens.Add(cancellationToken);
            return ValueTask.FromResult(result);
        }
    }
}
