using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Categories;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Categories;

public class CategoryBudgetEntityConfiguration : IEntityTypeConfiguration<CategoryBudget>
{
    public void Configure(EntityTypeBuilder<CategoryBudget> builder)
    {
        builder.ToTable("category_budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.CategoryId)
            .IsRequired();

        builder.Property(b => b.UserId)
            .IsRequired();

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

            money.WithOwner().HasForeignKey("Id");
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

            money.WithOwner().HasForeignKey("Id");
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

            range.WithOwner().HasForeignKey("Id");
        });

        builder.Property(b => b.AlertEnabled)
            .HasDefaultValue(true);

        builder.OwnsOne(b => b.AlertThreshold, pct =>
        {
            pct.Property(p => p.Value)
                .HasColumnName("alert_threshold")
                .HasColumnType("decimal(5,2)");

            pct.WithOwner().HasForeignKey("Id");
        });

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(b => new { b.UserId, b.CategoryId })
            .HasDatabaseName("ix_category_budgets_user_category");
    }
}
