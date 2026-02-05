using System.Text;
using ClosedXML.Excel;
using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Domain.Common;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ManageMyMoney.Infrastructure.Shared.Services.Export;

public class ExcelExportService : IExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<OperationResult<byte[]>> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            var dataList = data.ToList();

            if (!dataList.Any())
            {
                worksheet.Cell(1, 1).Value = "No data available";
                return await SaveWorkbookAsync(workbook);
            }

            var properties = typeof(T).GetProperties();

            // Headers
            for (var i = 0; i < properties.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = FormatHeader(properties[i].Name);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            // Data rows
            var row = 2;
            foreach (var item in dataList)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item);
                    var cell = worksheet.Cell(row, i + 1);
                    SetCellValue(cell, value);
                }
                row++;
            }

            worksheet.Columns().AdjustToContents();
            return await SaveWorkbookAsync(workbook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel");
            return OperationResult.Failure<byte[]>($"Failed to export to Excel: {ex.Message}");
        }
    }

    public async Task<OperationResult<byte[]>> ExportToPdfAsync(string htmlContent, string title)
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text(title)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Item().Text(htmlContent);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            var bytes = document.GeneratePdf();
            return await Task.FromResult(OperationResult.Success(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to PDF");
            return OperationResult.Failure<byte[]>($"Failed to export to PDF: {ex.Message}");
        }
    }

    public async Task<OperationResult<byte[]>> ExportToCsvAsync<T>(IEnumerable<T> data)
    {
        try
        {
            var dataList = data.ToList();
            var properties = typeof(T).GetProperties();
            var csv = new StringBuilder();

            // Headers
            csv.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

            // Data
            foreach (var item in dataList)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return EscapeCsvField(FormatValue(value));
                });
                csv.AppendLine(string.Join(",", values));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return await Task.FromResult(OperationResult.Success(bytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to CSV");
            return OperationResult.Failure<byte[]>($"Failed to export to CSV: {ex.Message}");
        }
    }

    public async Task<OperationResult<byte[]>> ExportExpenseReportToExcelAsync(
        IEnumerable<object> expenses,
        string userName,
        DateTime fromDate,
        DateTime toDate)
    {
        try
        {
            using var workbook = new XLWorkbook();
            
            // Summary sheet
            var summarySheet = workbook.Worksheets.Add("Summary");
            summarySheet.Cell(1, 1).Value = "Expense Report";
            summarySheet.Cell(1, 1).Style.Font.Bold = true;
            summarySheet.Cell(1, 1).Style.Font.FontSize = 16;

            summarySheet.Cell(3, 1).Value = "Generated for:";
            summarySheet.Cell(3, 2).Value = userName;
            summarySheet.Cell(4, 1).Value = "Period:";
            summarySheet.Cell(4, 2).Value = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";
            summarySheet.Cell(5, 1).Value = "Generated on:";
            summarySheet.Cell(5, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

            // Details sheet
            var detailsSheet = workbook.Worksheets.Add("Details");
            var expenseList = expenses.ToList();

            if (expenseList.Any())
            {
                var properties = expenseList.First().GetType().GetProperties();

                for (var i = 0; i < properties.Length; i++)
                {
                    var cell = detailsSheet.Cell(1, i + 1);
                    cell.Value = FormatHeader(properties[i].Name);
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                var row = 2;
                foreach (var expense in expenseList)
                {
                    for (var i = 0; i < properties.Length; i++)
                    {
                        var value = properties[i].GetValue(expense);
                        SetCellValue(detailsSheet.Cell(row, i + 1), value);
                    }
                    row++;
                }

                detailsSheet.Columns().AdjustToContents();
            }

            summarySheet.Columns().AdjustToContents();
            return await SaveWorkbookAsync(workbook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting expense report");
            return OperationResult.Failure<byte[]>($"Failed to export report: {ex.Message}");
        }
    }

    private static async Task<OperationResult<byte[]>> SaveWorkbookAsync(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(OperationResult.Success(stream.ToArray()));
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case DateTime dateValue:
                cell.Value = dateValue;
                cell.Style.NumberFormat.Format = "yyyy-MM-dd";
                break;
            case DateOnly dateOnlyValue:
                cell.Value = dateOnlyValue.ToDateTime(TimeOnly.MinValue);
                cell.Style.NumberFormat.Format = "yyyy-MM-dd";
                break;
            case bool boolValue:
                cell.Value = boolValue ? "Yes" : "No";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string FormatHeader(string name)
    {
        var result = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result.Append(' ');
            result.Append(c);
        }
        return result.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            decimal dec => dec.ToString("F2"),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
