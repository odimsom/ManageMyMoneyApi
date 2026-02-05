using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Income;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Income;

public class RecurringIncomeEntityConfiguration : IEntityTypeConfiguration<RecurringIncome>
{
    public void Configure(EntityTypeBuilder<RecurringIncome> builder)
    {
        builder.ToTable("recurring_incomes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(r => r.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.Recurrence)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.DayOfMonth)
            .IsRequired();

        builder.Property(r => r.IncomeSourceId)
            .IsRequired();

        builder.Property(r => r.AccountId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.EndDate);

        builder.Property(r => r.LastGeneratedDate);

        builder.Property(r => r.NextDueDate);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("ix_recurring_incomes_user_id");

        builder.HasIndex(r => new { r.UserId, r.IsActive })
            .HasDatabaseName("ix_recurring_incomes_user_active");
    }
}
