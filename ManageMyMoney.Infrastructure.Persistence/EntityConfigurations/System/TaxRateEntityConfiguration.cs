using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.System;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.System;

public class TaxRateEntityConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("tax_rates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.OwnsOne(t => t.Rate, pct =>
        {
            pct.Property(p => p.Value)
                .HasColumnName("rate")
                .IsRequired()
                .HasColumnType("decimal(5,2)");
        });

        builder.Property(t => t.CountryCode)
            .HasMaxLength(2);

        builder.Property(t => t.CategoryCode)
            .HasMaxLength(50);

        builder.Property(t => t.EffectiveFrom)
            .IsRequired();

        builder.Property(t => t.EffectiveTo);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => t.CountryCode)
            .HasDatabaseName("ix_tax_rates_country");

        builder.HasIndex(t => new { t.CountryCode, t.IsActive })
            .HasDatabaseName("ix_tax_rates_country_active");
    }
}
