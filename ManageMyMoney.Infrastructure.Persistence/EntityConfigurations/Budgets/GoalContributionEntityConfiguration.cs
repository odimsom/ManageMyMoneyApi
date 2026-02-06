using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Budgets;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Budgets;

public class GoalContributionEntityConfiguration : IEntityTypeConfiguration<GoalContribution>
{
    public void Configure(EntityTypeBuilder<GoalContribution> builder)
    {
        builder.ToTable("goal_contributions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.SavingsGoalId)
            .IsRequired();

        builder.OwnsOne(c => c.Amount, money =>
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

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.Property(c => c.Date)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.SavingsGoalId)
            .HasDatabaseName("ix_goal_contributions_goal_id");

        builder.HasIndex(c => c.Date)
            .HasDatabaseName("ix_goal_contributions_date");
    }
}
