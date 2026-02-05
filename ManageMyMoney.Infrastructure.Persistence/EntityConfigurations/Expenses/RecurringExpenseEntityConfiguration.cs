using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Expenses;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class RecurringExpenseEntityConfiguration : IEntityTypeConfiguration<RecurringExpense>
{
    public void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        builder.ToTable("recurring_expenses");

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

        builder.Property(r => r.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(r => r.CategoryId)
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
            .HasDatabaseName("ix_recurring_expenses_user_id");

        builder.HasIndex(r => new { r.UserId, r.IsActive })
            .HasDatabaseName("ix_recurring_expenses_user_active");

        builder.HasIndex(r => r.NextDueDate)
            .HasDatabaseName("ix_recurring_expenses_next_due");
    }
}
