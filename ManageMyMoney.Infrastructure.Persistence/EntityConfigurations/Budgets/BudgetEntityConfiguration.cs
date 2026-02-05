using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Budgets;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Budgets;

public class BudgetEntityConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.OwnsOne(b => b.Limit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("limit_amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("limit_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.OwnsOne(b => b.Spent, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("spent_amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("spent_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(b => b.Period)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(b => b.DateRange, range =>
        {
            range.Property(r => r.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            range.Property(r => r.EndDate)
                .HasColumnName("end_date")
                .IsRequired();
        });

        builder.Property(b => b.UserId)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true);

        builder.Property(b => b.AlertsEnabled)
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.UpdatedAt);

        // Store category IDs as JSON array
        builder.Property("_categoryIds")
            .HasColumnName("category_ids")
            .HasColumnType("jsonb");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("ix_budgets_user_id");

        builder.HasIndex(b => new { b.UserId, b.IsActive })
            .HasDatabaseName("ix_budgets_user_active");
    }
}
