using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Common.Interfaces;

public interface IExportService
{
    Task<OperationResult<byte[]>> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName);
    Task<OperationResult<byte[]>> ExportToPdfAsync(string htmlContent, string title);
    Task<OperationResult<byte[]>> ExportToCsvAsync<T>(IEnumerable<T> data);
    Task<OperationResult<byte[]>> ExportExpenseReportToExcelAsync(
        IEnumerable<object> expenses, 
        string userName, 
        DateTime fromDate, 
        DateTime toDate);
}
