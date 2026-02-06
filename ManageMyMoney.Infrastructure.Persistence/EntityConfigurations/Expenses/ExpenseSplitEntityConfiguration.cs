using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Expenses;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class ExpenseSplitEntityConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
    public void Configure(EntityTypeBuilder<ExpenseSplit> builder)
    {
        builder.ToTable("expense_splits");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.ExpenseId)
            .IsRequired();

        builder.Property(s => s.ParticipantUserId)
            .IsRequired();

        builder.OwnsOne(s => s.Amount, money =>
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

        builder.OwnsOne(s => s.Percentage, pct =>
        {
            pct.Property(p => p.Value)
                .HasColumnName("percentage")
                .HasColumnType("decimal(5,2)");

            pct.WithOwner().HasForeignKey("Id");
        });

        builder.Property(s => s.IsPaid)
            .HasDefaultValue(false);

        builder.Property(s => s.PaidAt);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(s => s.ExpenseId)
            .HasDatabaseName("ix_expense_splits_expense_id");

        builder.HasIndex(s => s.ParticipantUserId)
            .HasDatabaseName("ix_expense_splits_participant_id");
    }
}
