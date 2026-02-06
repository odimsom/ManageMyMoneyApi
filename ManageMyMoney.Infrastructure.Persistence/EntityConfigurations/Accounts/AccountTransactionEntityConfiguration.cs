using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Accounts;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Accounts;

public class AccountTransactionEntityConfiguration : IEntityTypeConfiguration<AccountTransaction>
{
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.ToTable("account_transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.FromAccountId)
            .IsRequired();

        builder.Property(t => t.ToAccountId)
            .IsRequired();

        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .IsRequired()
                .HasMaxLength(3);

            money.WithOwner().HasForeignKey("Id");
        });

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Date)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_account_transactions_user_id");

        builder.HasIndex(t => t.Date)
            .HasDatabaseName("ix_account_transactions_date");

        builder.HasIndex(t => t.FromAccountId)
            .HasDatabaseName("ix_account_transactions_from_account");

        builder.HasIndex(t => t.ToAccountId)
            .HasDatabaseName("ix_account_transactions_to_account");
    }
}
