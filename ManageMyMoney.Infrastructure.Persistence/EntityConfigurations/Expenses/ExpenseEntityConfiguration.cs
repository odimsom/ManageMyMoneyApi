using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Expenses;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class ExpenseEntityConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(e => e.Amount, money =>
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

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .IsRequired();

        builder.Property(e => e.SubcategoryId);

        builder.Property(e => e.AccountId)
            .IsRequired();

        builder.Property(e => e.PaymentMethodId);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Location)
            .HasMaxLength(300);

        builder.Property(e => e.IsRecurring)
            .HasDefaultValue(false);

        builder.Property(e => e.RecurringExpenseId);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt);

        // Indexes
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

        // Navigation - Tags (many-to-many handled via join table)
        builder.HasMany(e => e.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "expense_tag_mappings",
                j => j.HasOne<ExpenseTag>().WithMany().HasForeignKey("tag_id"),
                j => j.HasOne<Expense>().WithMany().HasForeignKey("expense_id")
            );

        builder.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey(a => a.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
