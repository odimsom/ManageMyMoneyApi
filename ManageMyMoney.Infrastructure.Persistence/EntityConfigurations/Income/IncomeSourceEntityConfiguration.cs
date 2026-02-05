using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Income;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Income;

public class IncomeSourceEntityConfiguration : IEntityTypeConfiguration<IncomeSource>
{
    public void Configure(EntityTypeBuilder<IncomeSource> builder)
    {
        builder.ToTable("income_sources");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Icon)
            .HasMaxLength(50);

        builder.Property(s => s.Color)
            .HasMaxLength(7);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(s => new { s.UserId, s.Name })
            .IsUnique()
            .HasDatabaseName("ix_income_sources_user_name");
    }
}
