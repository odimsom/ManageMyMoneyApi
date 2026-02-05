using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Expenses;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class ExpenseTagEntityConfiguration : IEntityTypeConfiguration<ExpenseTag>
{
    public void Configure(EntityTypeBuilder<ExpenseTag> builder)
    {
        builder.ToTable("expense_tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(t => t.Color)
            .HasMaxLength(7);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => new { t.UserId, t.Name })
            .IsUnique()
            .HasDatabaseName("ix_expense_tags_user_name");
    }
}
