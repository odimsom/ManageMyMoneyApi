using ManageMyMoney.Core.Application.DTOs.Reports;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Reports;

public interface IReportService
{
    // Summaries
    Task<OperationResult<FinancialSummaryResponse>> GetFinancialSummaryAsync(Guid userId, DateTime fromDate, DateTime toDate);
    Task<OperationResult<MonthlyReportResponse>> GetMonthlyReportAsync(Guid userId, int year, int month);
    Task<OperationResult<YearlyReportResponse>> GetYearlyReportAsync(Guid userId, int year);
    Task<OperationResult<IEnumerable<BudgetPerformanceResponse>>> GetBudgetPerformanceAsync(Guid userId);

    // Comparisons
    Task<OperationResult<ComparisonReportResponse>> GetComparisonReportAsync(Guid userId, ComparisonReportRequest request);
    Task<OperationResult<ComparisonReportResponse>> GetMonthOverMonthComparisonAsync(Guid userId, int year, int month);
    Task<OperationResult<ComparisonReportResponse>> GetYearOverYearComparisonAsync(Guid userId, int year);

    // Trends
    Task<OperationResult<IEnumerable<MonthlyTrendItem>>> GetExpenseTrendsAsync(Guid userId, int months = 12);
    Task<OperationResult<IEnumerable<MonthlyTrendItem>>> GetIncomeTrendsAsync(Guid userId, int months = 12);
    Task<OperationResult<IEnumerable<CategoryBreakdownItem>>> GetTopExpenseCategoriesAsync(Guid userId, DateTime fromDate, DateTime toDate, int top = 5);

    // Exports
    Task<OperationResult<byte[]>> ExportReportAsync(Guid userId, ExportReportRequest request);
    Task<OperationResult> SendMonthlyReportEmailAsync(Guid userId, int year, int month);
}
