using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Income;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Income;

public interface IIncomeService
{
    // CRUD Operations
    Task<OperationResult<IncomeResponse>> CreateIncomeAsync(Guid userId, CreateIncomeRequest request);
    Task<OperationResult<IncomeResponse>> GetIncomeByIdAsync(Guid userId, Guid incomeId);
    Task<OperationResult<PaginatedResponse<IncomeResponse>>> GetIncomesAsync(Guid userId, DateRangeRequest? dateRange, int pageNumber = 1, int pageSize = 20);
    Task<OperationResult<IncomeResponse>> UpdateIncomeAsync(Guid userId, Guid incomeId, UpdateIncomeRequest request);
    Task<OperationResult> DeleteIncomeAsync(Guid userId, Guid incomeId);

    // Summaries
    Task<OperationResult<IncomeSummaryResponse>> GetMonthlySummaryAsync(Guid userId, int year, int month);
    Task<OperationResult<IEnumerable<IncomeSourceResponse>>> GetIncomeBySourceAsync(Guid userId, DateTime fromDate, DateTime toDate);

    // Income Sources
    Task<OperationResult<IncomeSourceResponse>> CreateIncomeSourceAsync(Guid userId, CreateIncomeSourceRequest request);
    Task<OperationResult<IEnumerable<IncomeSourceResponse>>> GetIncomeSourcesAsync(Guid userId);
    Task<OperationResult<IncomeSourceResponse>> UpdateIncomeSourceAsync(Guid userId, Guid sourceId, CreateIncomeSourceRequest request);
    Task<OperationResult> DeleteIncomeSourceAsync(Guid userId, Guid sourceId);

    // Recurring Income
    Task<OperationResult<IncomeResponse>> CreateRecurringIncomeAsync(Guid userId, CreateRecurringIncomeRequest request);
    Task<OperationResult<IEnumerable<IncomeResponse>>> GetRecurringIncomesAsync(Guid userId);

    // Export
    Task<OperationResult<byte[]>> ExportToExcelAsync(Guid userId, DateTime fromDate, DateTime toDate);
}
