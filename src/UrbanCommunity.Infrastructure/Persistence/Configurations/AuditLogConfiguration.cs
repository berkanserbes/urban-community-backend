using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : BaseEntityConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        base.Configure(builder);

        builder.Property(value => value.Action).HasMaxLength(20).IsRequired();
        builder.Property(value => value.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(value => value.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(value => value.OldValues).HasColumnType("jsonb");
        builder.Property(value => value.NewValues).HasColumnType("jsonb");

        builder.HasIndex(value => value.ActorUserId);
        builder.HasIndex(value => new { value.EntityType, value.EntityId });
        builder.HasIndex(value => value.CreatedAt);
    }
}
