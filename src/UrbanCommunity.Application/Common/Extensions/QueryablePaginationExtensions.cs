using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Common.Extensions;

public static class QueryablePaginationExtensions
{
    /// <summary>
    /// Counts the filtered query, then fetches one page in SQL. The caller must
    /// apply a deterministic ordering (including a unique tie breaker) before projection.
    /// User input is validated by <c>ValidationBehavior</c>; these guards only catch programming errors.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageIndex, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, PaginationRequest.MaxPageSize);

        var offset = ((long)pageIndex - 1) * pageSize;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, int.MaxValue, nameof(pageIndex));
        cancellationToken.ThrowIfCancellationRequested();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((int)offset).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageIndex, pageSize, totalCount);
    }
}
