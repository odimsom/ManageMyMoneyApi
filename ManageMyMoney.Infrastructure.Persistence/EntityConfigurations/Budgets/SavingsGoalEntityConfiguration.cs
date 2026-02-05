using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Budgets;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Budgets;

public class SavingsGoalEntityConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.ToTable("savings_goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Description)
            .HasMaxLength(500);

        builder.OwnsOne(g => g.TargetAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("target_amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("target_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.OwnsOne(g => g.CurrentAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("current_amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("current_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(g => g.TargetDate);

        builder.Property(g => g.Icon)
            .HasMaxLength(50);

        builder.Property(g => g.Color)
            .HasMaxLength(7);

        builder.Property(g => g.UserId)
            .IsRequired();

        builder.Property(g => g.LinkedAccountId);

        builder.Property(g => g.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(g => g.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(g => g.CompletedAt);

        builder.HasMany(g => g.Contributions)
            .WithOne()
            .HasForeignKey(c => c.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.UserId)
            .HasDatabaseName("ix_savings_goals_user_id");

        builder.HasIndex(g => new { g.UserId, g.Status })
            .HasDatabaseName("ix_savings_goals_user_status");
    }
}
