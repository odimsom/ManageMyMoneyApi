using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Categories;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Categories;

public class SubcategoryEntityConfiguration : IEntityTypeConfiguration<Subcategory>
{
    public void Configure(EntityTypeBuilder<Subcategory> builder)
    {
        builder.ToTable("subcategories");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Icon)
            .HasMaxLength(50);

        builder.Property(s => s.CategoryId)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(s => new { s.CategoryId, s.Name })
            .IsUnique()
            .HasDatabaseName("ix_subcategories_category_name");
    }
}
