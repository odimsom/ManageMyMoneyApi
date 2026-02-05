using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Expenses;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Expenses;

public interface IExpenseService
{
    // CRUD Operations
    Task<OperationResult<ExpenseResponse>> CreateExpenseAsync(Guid userId, CreateExpenseRequest request);
    Task<OperationResult<ExpenseResponse>> CreateQuickExpenseAsync(Guid userId, CreateQuickExpenseRequest request);
    Task<OperationResult<ExpenseResponse>> GetExpenseByIdAsync(Guid userId, Guid expenseId);
    Task<OperationResult<PaginatedResponse<ExpenseResponse>>> GetExpensesAsync(Guid userId, ExpenseFilterRequest filter);
    Task<OperationResult<ExpenseResponse>> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseRequest request);
    Task<OperationResult> DeleteExpenseAsync(Guid userId, Guid expenseId);

    // Summaries
    Task<OperationResult<ExpenseSummaryResponse>> GetMonthlySummaryAsync(Guid userId, int year, int month);
    Task<OperationResult<IEnumerable<CategoryExpenseSummary>>> GetSummaryByCategoryAsync(Guid userId, DateTime fromDate, DateTime toDate);
    Task<OperationResult<IEnumerable<DailyExpenseSummary>>> GetDailySummaryAsync(Guid userId, DateTime fromDate, DateTime toDate);

    // Tags
    Task<OperationResult<TagResponse>> CreateTagAsync(Guid userId, CreateExpenseTagRequest request);
    Task<OperationResult<IEnumerable<TagResponse>>> GetTagsAsync(Guid userId);
    Task<OperationResult> DeleteTagAsync(Guid userId, Guid tagId);
    Task<OperationResult> AddTagToExpenseAsync(Guid userId, Guid expenseId, Guid tagId);
    Task<OperationResult> RemoveTagFromExpenseAsync(Guid userId, Guid expenseId, Guid tagId);

    // Attachments
    Task<OperationResult<AttachmentResponse>> AddAttachmentAsync(Guid userId, Guid expenseId, Stream fileStream, string fileName, string contentType);
    Task<OperationResult> DeleteAttachmentAsync(Guid userId, Guid expenseId, Guid attachmentId);

    // Recurring Expenses
    Task<OperationResult<RecurringExpenseResponse>> CreateRecurringExpenseAsync(Guid userId, CreateRecurringExpenseRequest request);
    Task<OperationResult<IEnumerable<RecurringExpenseResponse>>> GetRecurringExpensesAsync(Guid userId);
    Task<OperationResult> PauseRecurringExpenseAsync(Guid userId, Guid recurringExpenseId);
    Task<OperationResult> ResumeRecurringExpenseAsync(Guid userId, Guid recurringExpenseId);
    Task<OperationResult> DeleteRecurringExpenseAsync(Guid userId, Guid recurringExpenseId);

    // Export
    Task<OperationResult<byte[]>> ExportToExcelAsync(Guid userId, DateTime fromDate, DateTime toDate);
    Task<OperationResult<byte[]>> ExportToCsvAsync(Guid userId, DateTime fromDate, DateTime toDate);
}
