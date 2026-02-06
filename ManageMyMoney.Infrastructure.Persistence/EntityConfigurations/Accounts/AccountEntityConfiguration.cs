using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Accounts;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Accounts;

public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(a => a.Balance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("balance")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("balance_currency")
                .IsRequired()
                .HasMaxLength(3);
                
            // Ensure this owned entity doesn't create its own key
            money.HasNoKey();
        });

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(a => a.Icon)
            .HasMaxLength(50);

        builder.Property(a => a.Color)
            .HasMaxLength(7);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.IncludeInTotal)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.UpdatedAt);

        builder.HasIndex(a => new { a.UserId, a.Name })
            .IsUnique()
            .HasDatabaseName("ix_accounts_user_name");

        builder.HasIndex(a => new { a.UserId, a.IsActive })
            .HasDatabaseName("ix_accounts_user_active");
    }
}
