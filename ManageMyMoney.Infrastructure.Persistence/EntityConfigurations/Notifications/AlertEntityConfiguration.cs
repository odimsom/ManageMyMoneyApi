using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Notifications;

public class AlertEntityConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AlertType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.RelatedEntityType)
            .HasMaxLength(50);

        builder.Property(a => a.RelatedEntityId);

        builder.Property(a => a.IsAcknowledged)
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.AcknowledgedAt);

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_alerts_user_id");

        builder.HasIndex(a => new { a.UserId, a.IsAcknowledged })
            .HasDatabaseName("ix_alerts_user_acknowledged");

        builder.HasIndex(a => a.AlertType)
            .HasDatabaseName("ix_alerts_type");
    }
}
