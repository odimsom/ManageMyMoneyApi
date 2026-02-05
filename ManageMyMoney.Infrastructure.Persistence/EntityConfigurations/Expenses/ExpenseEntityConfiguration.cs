using ManageMyMoney.Core.Domain.Entities.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class ExpenseEntityConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(e => e.Location)
            .HasColumnName("location")
            .HasMaxLength(200);

        builder.Property(e => e.IsRecurring)
            .HasColumnName("is_recurring")
            .HasDefaultValue(false);

        builder.Property(e => e.RecurringExpenseId)
            .HasColumnName("recurring_expense_id");

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(e => e.SubcategoryId)
            .HasColumnName("subcategory_id");

        builder.Property(e => e.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(e => e.PaymentMethodId)
            .HasColumnName("payment_method_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.OwnsOne(e => e.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Subcategory)
            .WithMany()
            .HasForeignKey(e => e.SubcategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Account)
            .WithMany(a => a.Expenses)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PaymentMethod)
            .WithMany()
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.User)
            .WithMany(u => u.Expenses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RecurringExpense)
            .WithMany()
            .HasForeignKey(e => e.RecurringExpenseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_expenses_user_id");

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("ix_expenses_date");

        builder.HasIndex(e => new { e.UserId, e.Date })
            .HasDatabaseName("ix_expenses_user_date");

        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("ix_expenses_category_id");

        builder.HasIndex(e => e.AccountId)
            .HasDatabaseName("ix_expenses_account_id");
    }
}
