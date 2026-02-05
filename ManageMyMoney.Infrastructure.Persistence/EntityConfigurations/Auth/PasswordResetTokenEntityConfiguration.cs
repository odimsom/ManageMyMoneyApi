using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Auth;

public class PasswordResetTokenEntityConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(p => p.Token)
            .IsUnique()
            .HasDatabaseName("ix_password_reset_tokens_token");

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.IsUsed)
            .HasDefaultValue(false);

        builder.Property(p => p.UsedAt);

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_password_reset_tokens_user_id");
    }
}
