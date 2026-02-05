using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Auth;

public class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.Property(s => s.DeviceInfo)
            .HasMaxLength(200);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.LastActivityAt)
            .IsRequired();

        builder.Property(s => s.EndedAt);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ix_user_sessions_user_id");

        builder.HasIndex(s => new { s.UserId, s.IsActive })
            .HasDatabaseName("ix_user_sessions_user_active");
    }
}
