namespace UrbanCommunity.Application.Common.Models;

/// <summary>One-based pagination shared by list queries and HTTP query parameters.</summary>
public record PaginationRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
}
