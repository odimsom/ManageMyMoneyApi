using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Accounts;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Accounts;

public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.ToTable("credit_cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.AccountId)
            .IsRequired();

        builder.OwnsOne(c => c.CreditLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("credit_limit")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("credit_limit_currency")
                .IsRequired()
                .HasMaxLength(3);

            money.WithOwner().HasForeignKey("Id");
        });

        builder.OwnsOne(c => c.CurrentBalance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("current_balance")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("current_balance_currency")
                .IsRequired()
                .HasMaxLength(3);

            money.WithOwner().HasForeignKey("Id");
        });

        builder.Property(c => c.StatementClosingDay)
            .IsRequired();

        builder.Property(c => c.PaymentDueDay)
            .IsRequired();

        builder.OwnsOne(c => c.InterestRate, pct =>
        {
            pct.Property(p => p.Value)
                .HasColumnName("interest_rate")
                .HasColumnType("decimal(5,2)");

            pct.WithOwner().HasForeignKey("Id");
        });

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.AccountId)
            .IsUnique()
            .HasDatabaseName("ix_credit_cards_account_id");
    }
}
