using System.Linq.Expressions;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Common.Projections;

public static class UserProjections
{
    /// <summary>
    /// SQL-translatable equivalent of <see cref="User.FullName"/>. Use inside queries via
    /// <c>UserProjections.FullName.Invoke(entity.User)</c> and call <c>Expand()</c> on the query.
    /// </summary>
    public static readonly Expression<Func<User, string>> FullName = user =>
        string.IsNullOrWhiteSpace(user.FirstName)
            ? (string.IsNullOrWhiteSpace(user.LastName) ? "" : user.LastName)
            : string.IsNullOrWhiteSpace(user.LastName)
                ? user.FirstName
                : user.FirstName + " " + user.LastName;
}
