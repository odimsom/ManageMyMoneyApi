namespace ManageMyMoney.Core.Application.DTOs.Expenses;

public record CreateExpenseRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public required string Description { get; init; }
    public DateTime Date { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public Guid AccountId { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public string? Notes { get; init; }
    public string? Location { get; init; }
    public List<Guid>? TagIds { get; init; }
}

public record CreateQuickExpenseRequest
{
    public decimal Amount { get; init; }
    public required string Description { get; init; }
    public Guid? CategoryId { get; init; }
}

public record UpdateExpenseRequest
{
    public decimal? Amount { get; init; }
    public string? Description { get; init; }
    public DateTime? Date { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public string? Notes { get; init; }
    public string? Location { get; init; }
}

public record ExpenseResponse
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Description { get; init; }
    public DateTime Date { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public string? CategoryIcon { get; init; }
    public string? CategoryColor { get; init; }
    public Guid? SubcategoryId { get; init; }
    public string? SubcategoryName { get; init; }
    public Guid AccountId { get; init; }
    public required string AccountName { get; init; }
    public string? Notes { get; init; }
    public string? Location { get; init; }
    public bool IsRecurring { get; init; }
    public List<TagResponse>? Tags { get; init; }
    public List<AttachmentResponse>? Attachments { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ExpenseFilterRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? AccountId { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string? SearchTerm { get; init; }
    public List<Guid>? TagIds { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record ExpenseSummaryResponse
{
    public decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public int ExpenseCount { get; init; }
    public decimal AverageExpense { get; init; }
    public decimal HighestExpense { get; init; }
    public decimal LowestExpense { get; init; }
    public decimal DailyAverage { get; init; }
    public required string TopCategoryName { get; init; }
    public decimal TopCategoryAmount { get; init; }
}

public record CategoryExpenseSummary
{
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public string? CategoryIcon { get; init; }
    public string? CategoryColor { get; init; }
    public decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public int ExpenseCount { get; init; }
    public decimal Percentage { get; init; }
}

public record DailyExpenseSummary
{
    public DateTime Date { get; init; }
    public decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public int ExpenseCount { get; init; }
}

public record TagResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Color { get; init; }
}

public record AttachmentResponse
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public required string ContentType { get; init; }
    public long FileSizeBytes { get; init; }
    public DateTime UploadedAt { get; init; }
}

public record CreateExpenseTagRequest
{
    public required string Name { get; init; }
    public string? Color { get; init; }
}

public record CreateRecurringExpenseRequest
{
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public required string Recurrence { get; init; }
    public int DayOfMonth { get; init; } = 1;
    public Guid CategoryId { get; init; }
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record RecurringExpenseResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? Description { get; init; }
    public required string Recurrence { get; init; }
    public int DayOfMonth { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? NextDueDate { get; init; }
    public bool IsActive { get; init; }
}
