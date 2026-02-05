using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.System;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.System;

public class ExchangeRateEntityConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.FromCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.ToCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Rate)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.EffectiveDate)
            .IsRequired();

        builder.Property(e => e.Source)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(e => new { e.FromCurrency, e.ToCurrency, e.EffectiveDate })
            .HasDatabaseName("ix_exchange_rates_currencies_date");

        builder.HasIndex(e => e.EffectiveDate)
            .HasDatabaseName("ix_exchange_rates_effective_date");
    }
}
