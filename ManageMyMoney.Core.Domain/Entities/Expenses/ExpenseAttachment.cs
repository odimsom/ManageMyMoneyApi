using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class ExpenseAttachment
{
    public Guid Id { get; private set; }
    public Guid ExpenseId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private ExpenseAttachment() { }

    public static OperationResult<ExpenseAttachment> Create(
        Guid expenseId,
        string fileName,
        string fileUrl,
        string contentType,
        long fileSizeBytes)
    {
        if (expenseId == Guid.Empty)
            return OperationResult.Failure<ExpenseAttachment>("Expense ID is required");

        if (string.IsNullOrWhiteSpace(fileName))
            return OperationResult.Failure<ExpenseAttachment>("File name is required");

        if (string.IsNullOrWhiteSpace(fileUrl))
            return OperationResult.Failure<ExpenseAttachment>("File URL is required");

        if (fileSizeBytes <= 0)
            return OperationResult.Failure<ExpenseAttachment>("Invalid file size");

        // 10MB limit
        if (fileSizeBytes > 10 * 1024 * 1024)
            return OperationResult.Failure<ExpenseAttachment>("File size cannot exceed 10MB");

        var attachment = new ExpenseAttachment
        {
            Id = Guid.NewGuid(),
            ExpenseId = expenseId,
            FileName = fileName,
            FileUrl = fileUrl,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            UploadedAt = DateTime.UtcNow
        };

        return OperationResult.Success(attachment);
    }
}
