using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Notifications;

public class ReminderEntityConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("reminders");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.DueDate)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.RelatedEntityType)
            .HasMaxLength(50);

        builder.Property(r => r.RelatedEntityId);

        builder.Property(r => r.IsRecurring)
            .HasDefaultValue(false);

        builder.Property(r => r.Recurrence)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(r => r.IsSent)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_reminders_user_id");

        builder.HasIndex(r => r.DueDate)
            .HasDatabaseName("ix_reminders_due_date");

        builder.HasIndex(r => new { r.UserId, r.IsCompleted })
            .HasDatabaseName("ix_reminders_user_completed");
    }
}
