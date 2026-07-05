using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetTopologySuite.Geometries;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence;

public sealed class AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    : SaveChangesInterceptor
{
    private static readonly Type[] AuditedTypes =
    [
        typeof(Announcement),
        typeof(CommunityPost),
        typeof(Department),
        typeof(Organization),
        typeof(OrganizationDepartment),
        typeof(OrganizationDepartmentCategory),
        typeof(Report),
        typeof(ReportCategory),
        typeof(Suggestion),
        typeof(User)
    ];

    private static readonly HashSet<string> SensitiveProperties =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "TokenHash",
            "RefreshToken",
            "AccessToken"
        };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void ApplyAudit(DbContext context)
    {
        var now = DateTime.UtcNow;
        ApplyTimestamps(context, now);

        var actorUserId = Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var parsedActorId)
            ? parsedActorId
            : (Guid?)null;

        var logs = context.ChangeTracker.Entries()
            .Where(IsAuditable)
            .Select(entry => CreateAuditLog(entry, actorUserId))
            .Where(log => log is not null)
            .Cast<AuditLog>()
            .ToList();

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    private static bool IsAuditable(EntityEntry entry) =>
        entry.Entity is not AuditLog &&
        AuditedTypes.Contains(entry.Entity.GetType()) &&
        entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    private static AuditLog? CreateAuditLog(
        EntityEntry entry,
        Guid? actorUserId)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties.Where(property =>
                     !SensitiveProperties.Contains(property.Metadata.Name)))
        {
            if (entry.State == EntityState.Modified && !property.IsModified)
                continue;

            if (entry.State is EntityState.Modified or EntityState.Deleted)
                oldValues[property.Metadata.Name] = NormalizeAuditValue(property.OriginalValue);
            if (entry.State is EntityState.Modified or EntityState.Added)
                newValues[property.Metadata.Name] = NormalizeAuditValue(property.CurrentValue);
        }

        if (oldValues.Count == 0 && newValues.Count == 0)
            return null;

        return new AuditLog
        {
            ActorUserId = actorUserId,
            Action = entry.State.ToString(),
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = ResolveEntityId(entry),
            OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
            NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
        };
    }

    private static object? NormalizeAuditValue(object? value)
    {
        return value switch
        {
            Point point => new Dictionary<string, object?>
            {
                ["type"] = point.GeometryType,
                ["srid"] = point.SRID,
                ["longitude"] = ToFiniteOrNull(point.X),
                ["latitude"] = ToFiniteOrNull(point.Y),
                ["wkt"] = point.AsText()
            },
            Geometry geometry => new Dictionary<string, object?>
            {
                ["type"] = geometry.GeometryType,
                ["srid"] = geometry.SRID,
                ["wkt"] = geometry.AsText()
            },
            double doubleValue => ToFiniteOrNull(doubleValue),
            float floatValue => float.IsFinite(floatValue) ? floatValue : null,
            _ => value
        };
    }

    private static double? ToFiniteOrNull(double value) =>
        double.IsFinite(value) ? value : null;

    private static string ResolveEntityId(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey is null)
            return string.Empty;

        return string.Join(
            ":",
            primaryKey.Properties.Select(property =>
                entry.Property(property.Name).CurrentValue?.ToString() ?? string.Empty));
    }

    private static void ApplyTimestamps(DbContext context, DateTime now)
    {
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

        foreach (var entry in context.ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt(now);
        }
    }
}
