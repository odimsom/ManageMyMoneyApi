using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.System;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.System;

public class CurrencyEntityConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");

        builder.HasKey(c => c.Code);

        builder.Property(c => c.Code)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Symbol)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(c => c.DecimalPlaces)
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("ix_currencies_active");
    }
}
