using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Auth;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(256);

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("ix_users_email");
        });

        builder.OwnsOne(u => u.PasswordHash, password =>
        {
            password.Property(p => p.HashedValue)
                .HasColumnName("password_hash")
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("phone_number")
                .HasMaxLength(15);

            phone.Property(p => p.CountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(5);
        });

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(u => u.TimeZone)
            .HasMaxLength(50);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.IsTwoFactorEnabled)
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.LastLoginAt);

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("ix_users_is_active");
    }
}
