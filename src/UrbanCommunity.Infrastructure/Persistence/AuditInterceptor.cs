using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAuditTimestamps(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAuditTimestamps(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private static void ApplyAuditTimestamps(DbContext context)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreatedAt(now);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdatedAt(now);
                    break;
            }
        }

        // User inherits from IdentityUser, not BaseEntity — handled separately
        foreach (var entry in context.ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt(now);
        }
    }
}
