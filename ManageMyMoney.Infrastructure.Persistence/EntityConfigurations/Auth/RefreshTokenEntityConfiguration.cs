using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Auth;

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(r => r.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(r => r.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(r => r.RevokedAt);

        builder.Property(r => r.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500);

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        builder.HasIndex(r => r.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");
    }
}
