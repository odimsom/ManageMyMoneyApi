using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Auth;

public class EmailVerificationTokenEntityConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.Token)
            .IsUnique()
            .HasDatabaseName("ix_email_verification_tokens_token");

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.IsUsed)
            .HasDefaultValue(false);

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_email_verification_tokens_user_id");
    }
}
