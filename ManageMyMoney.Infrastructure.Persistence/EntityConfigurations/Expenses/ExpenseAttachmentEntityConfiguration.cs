using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ManageMyMoney.Core.Domain.Entities.Expenses;

namespace ManageMyMoney.Infrastructure.Persistence.EntityConfigurations.Expenses;

public class ExpenseAttachmentEntityConfiguration : IEntityTypeConfiguration<ExpenseAttachment>
{
    public void Configure(EntityTypeBuilder<ExpenseAttachment> builder)
    {
        builder.ToTable("expense_attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.ExpenseId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(a => a.ExpenseId)
            .HasDatabaseName("ix_expense_attachments_expense_id");
    }
}
