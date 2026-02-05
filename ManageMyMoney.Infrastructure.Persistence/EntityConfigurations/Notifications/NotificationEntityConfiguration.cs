using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Notifications;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(50);

        builder.Property(n => n.RelatedEntityId);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(n => n.ReadAt);

        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("ix_notifications_user_id");

        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("ix_notifications_user_read");

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("ix_notifications_created_at");
    }
}
