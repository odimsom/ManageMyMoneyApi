using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Income;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Income;

public class IncomeEntityConfiguration : IEntityTypeConfiguration<Core.Domain.Entities.Income.Income>
{
    public void Configure(EntityTypeBuilder<Core.Domain.Entities.Income.Income> builder)
    {
        builder.ToTable("incomes");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(i => i.Amount, money =>
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

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Date)
            .IsRequired();

        builder.Property(i => i.IncomeSourceId)
            .IsRequired();

        builder.Property(i => i.AccountId)
            .IsRequired();

        builder.Property(i => i.UserId)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.Property(i => i.IsRecurring)
            .HasDefaultValue(false);

        builder.Property(i => i.RecurringIncomeId);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(i => i.UpdatedAt);

        builder.HasIndex(i => i.UserId)
            .HasDatabaseName("ix_incomes_user_id");

        builder.HasIndex(i => i.Date)
            .HasDatabaseName("ix_incomes_date");

        builder.HasIndex(i => new { i.UserId, i.Date })
            .HasDatabaseName("ix_incomes_user_date");
    }
}
