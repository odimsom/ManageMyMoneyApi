using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Accounts;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Accounts;

public class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.LastFourDigits)
            .HasMaxLength(4);

        builder.Property(p => p.Icon)
            .HasMaxLength(50);

        builder.Property(p => p.AccountId);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.IsDefault)
            .HasDefaultValue(false);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_payment_methods_user_id");

        builder.HasIndex(p => new { p.UserId, p.IsDefault })
            .HasDatabaseName("ix_payment_methods_user_default");
    }
}
