using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Categories;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Categories;

public class CategoryEntityConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(200);

        builder.Property(c => c.Icon)
            .HasMaxLength(50);

        builder.Property(c => c.Color)
            .HasMaxLength(7);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.IsDefault)
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => new { c.UserId, c.Name })
            .IsUnique()
            .HasDatabaseName("ix_categories_user_name");

        builder.HasIndex(c => new { c.UserId, c.TransactionType })
            .HasDatabaseName("ix_categories_user_type");

        builder.HasMany(c => c.Subcategories)
            .WithOne()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
